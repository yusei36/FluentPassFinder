using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Contracts.Public.Ipc;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace FluentPassFinderPlugin.Ipc
{
    internal class PluginRequestHandler
    {
        private readonly IPluginHost pluginHost;
        private readonly MainForm mainWindow;
        private readonly Dispatcher pluginHostDispatcher;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Settings settings;
        private readonly Dictionary<int, byte[]> builtInIconCache = new Dictionary<int, byte[]>();

        public PluginRequestHandler(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            pluginHostDispatcher = Dispatcher.CurrentDispatcher;
            mainWindow = pluginHost.MainWindow;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            settings = LoadOrCreateDefaultSettings();
        }

        /// <summary>
        /// Dispatch a raw JSON request string to the appropriate handler.
        /// The <see cref="PipeEnvelope.Type"/> field is read first; then the JSON is
        /// deserialized again to the exact concrete request type — no JObject wrapper needed.
        /// </summary>
        public PipeEnvelope Handle(string requestJson)
        {
            // Peek at the base class to read Id and Type without full deserialization
            var peek = PipeProtocol.Deserialize<PipeEnvelope>(requestJson);

            try
            {
                switch (peek.Type)
                {
                    case PipeRequestTypes.SearchEntries:
                        return HandleSearchEntries(PipeProtocol.Deserialize<SearchEntriesRequest>(requestJson));
                    case PipeRequestTypes.GetPlaceholderValue:
                        return HandleGetPlaceholderValue(PipeProtocol.Deserialize<GetPlaceholderValueRequest>(requestJson));
                    case PipeRequestTypes.GetStringFromCustomConfig:
                        return HandleGetStringFromCustomConfig(PipeProtocol.Deserialize<GetStringFromCustomConfigRequest>(requestJson));
                    case PipeRequestTypes.GetSettings:
                        return new GetSettingsResponse { Id = peek.Id, Success = true, Settings = settings };
                    case PipeRequestTypes.IsAnyDatabaseOpen:
                        return HandleIsAnyDatabaseOpen(peek.Id);
                    case PipeRequestTypes.CopyField:
                        return HandleCopyField(PipeProtocol.Deserialize<CopyFieldRequest>(requestJson));
                    case PipeRequestTypes.CopyToClipboard:
                        return HandleCopyToClipboard(PipeProtocol.Deserialize<CopyToClipboardRequest>(requestJson));
                    case PipeRequestTypes.AutoTypeField:
                        return HandleAutoTypeField(PipeProtocol.Deserialize<AutoTypeFieldRequest>(requestJson));
                    case PipeRequestTypes.PerformAutoType:
                        return HandlePerformAutoType(PipeProtocol.Deserialize<PerformAutoTypeRequest>(requestJson));
                    case PipeRequestTypes.OpenEntryUrl:
                        return HandleOpenEntryUrl(PipeProtocol.Deserialize<OpenEntryUrlRequest>(requestJson));
                    case PipeRequestTypes.SelectEntry:
                        return HandleSelectEntry(PipeProtocol.Deserialize<SelectEntryRequest>(requestJson));
                    default:
                        return Ack(peek.Id, success: false, error: $"Unknown request type: {peek.Type}");
                }
            }
            catch (Exception ex)
            {
                return Ack(peek.Id, success: false, error: ex.Message);
            }
        }

        // ── Search ────────────────────────────────────────────────────────────────

        private PipeEnvelope HandleSearchEntries(SearchEntriesRequest req)
        {
            var entries = pluginHostDispatcher.Invoke(() =>
            {
                var query = (req.Query ?? string.Empty).ToLowerInvariant();
                var searchOptions = settings.SearchOptions;
                var searchTime = DateTime.Now;
                var results = new List<EntryDto>();

                foreach (var db in mainWindow.DocumentManager.GetOpenDatabases())
                {
                    var allGroups = CollectGroups(db.RootGroup);
                    var includedGroups = searchOptions.ExcludeGroupsBySearchSetting
                        ? allGroups.Where(g => g.GetSearchingEnabledInherited()).ToList()
                        : allGroups;

                    var dbUuid = UuidToString(db.RootGroup.Uuid);
                    foreach (var entry in includedGroups.SelectMany(g => g.GetEntries(false)))
                    {
                        if (searchOptions.ExcludeExpiredEntries && entry.Expires && searchTime > entry.ExpiryTime)
                            continue;

                        if (MatchesQuery(entry, db, query, searchOptions))
                            results.Add(BuildEntryDto(entry, db, dbUuid));
                    }
                }

                return results;
            });

            return new SearchEntriesResponse { Id = req.Id, Success = true, Entries = entries.ToArray() };
        }

        // ── Actions ───────────────────────────────────────────────────────────────

        private PipeEnvelope HandleGetPlaceholderValue(GetPlaceholderValueRequest req)
        {
            var value = pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return req.Placeholder;
                var flags = req.ResolveAll ? SprCompileFlags.All : SprCompileFlags.Deref;
                return SprEngine.Compile(req.Placeholder, new SprContext(entry, db, flags, true, false));
            });
            return new GetPlaceholderValueResponse { Id = req.Id, Success = true, Value = value };
        }

        private PipeEnvelope HandleGetStringFromCustomConfig(GetStringFromCustomConfigRequest req)
        {
            var value = pluginHostDispatcher.Invoke(() =>
                pluginHost.CustomConfig.GetString(req.ConfigId, req.DefaultValue));
            return new GetStringFromCustomConfigResponse { Id = req.Id, Success = true, Value = value };
        }

        private PipeEnvelope HandleIsAnyDatabaseOpen(string requestId)
        {
            var isOpen = pluginHostDispatcher.Invoke(() =>
                mainWindow.DocumentManager.GetOpenDatabases().Any());
            return new IsAnyDatabaseOpenResponse { Id = requestId, Success = true, IsOpen = isOpen };
        }

        private PipeEnvelope HandleCopyField(CopyFieldRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ack(req.Id);
        }

        private PipeEnvelope HandleCopyToClipboard(CopyToClipboardRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                if (ClipboardUtil.Copy(req.Value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ack(req.Id);
        }

        private PipeEnvelope HandleAutoTypeField(AutoTypeFieldRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                AutoType.PerformIntoCurrentWindow(entry, db, value + "{ENTER}");
            });
            return Ack(req.Id);
        }

        private PipeEnvelope HandlePerformAutoType(PerformAutoTypeRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null)
                    AutoType.PerformIntoCurrentWindow(entry, db, req.Sequence);
            });
            return Ack(req.Id);
        }

        private PipeEnvelope HandleOpenEntryUrl(OpenEntryUrlRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, _) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null) WinUtil.OpenEntryUrl(entry);
            });
            return Ack(req.Id);
        }

        private PipeEnvelope HandleSelectEntry(SelectEntryRequest req)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                mainWindow.UpdateUI(false, mainWindow.DocumentManager.FindDocument(db), true, entry.ParentGroup, true, null, false);
                mainWindow.SelectEntries(new PwObjectList<PwEntry> { entry }, true, true);
                mainWindow.EnsureVisibleEntry(entry.Uuid);
                mainWindow.UpdateUI(false, null, false, null, false, null, false);
                mainWindow.EnsureVisibleForegroundWindow(true, true);
            });
            return Ack(req.Id);
        }

        // ── Search helpers ────────────────────────────────────────────────────────

        private bool MatchesQuery(PwEntry entry, PwDatabase db, string query, SearchOptions opts)
        {
            foreach (var fieldName in GetSearchFieldNames(entry, opts))
            {
                var value = entry.Strings.ReadSafe(fieldName);
                if (opts.ResolveFieldReferences && value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                if (value.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }

            if (opts.IncludeTags)
            {
                foreach (var tag in entry.Tags)
                {
                    if (tag.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        private IEnumerable<string> GetSearchFieldNames(PwEntry entry, SearchOptions opts)
        {
            var fields = new List<string>();
            if (opts.IncludeTitleField)    fields.Add(PwDefs.TitleField);
            if (opts.IncludeUserNameField) fields.Add(PwDefs.UserNameField);
            if (opts.IncludePasswordField) fields.Add(PwDefs.PasswordField);
            if (opts.IncludeNotesField)    fields.Add(PwDefs.NotesField);
            if (opts.IncludeUrlField)      fields.Add(PwDefs.UrlField);

            if (opts.IncludeCustomFields)
            {
                var custom = entry.Strings.Where(s => !PwDefs.IsStandardField(s.Key));
                if (opts.IncludeProtectedCustomFields)
                    fields.AddRange(custom.Select(s => s.Key));
                else
                    fields.AddRange(custom.Where(s => !s.Value.IsProtected).Select(s => s.Key));
            }

            return fields;
        }

        private EntryDto BuildEntryDto(PwEntry entry, PwDatabase db, string dbUuid)
        {
            var resolve = settings.SearchOptions.ResolveFieldReferences;
            var dto = new EntryDto
            {
                Uuid     = UuidToString(entry.Uuid),
                DatabaseUuid = dbUuid,
                Title    = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.TitleField),    entry, db, resolve),
                UserName = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UserNameField), entry, db, resolve),
                Url      = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UrlField),      entry, db, resolve),
                Expires  = entry.Expires,
                Tags     = entry.Tags.ToList(),
                Icon     = GetEntryIconBytes(entry, db),
            };

            foreach (var field in entry.Strings)
            {
                dto.Fields[field.Key] = new EntryFieldDto
                {
                    IsProtected = field.Value.IsProtected,
                    HasValue    = field.Value.IsProtected
                                    || !string.IsNullOrWhiteSpace(field.Value.ReadString()),
                };
            }

            return dto;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private (PwEntry entry, PwDatabase database) ResolveEntry(string entryUuid, string databaseUuid)
        {
            if (string.IsNullOrEmpty(entryUuid) || string.IsNullOrEmpty(databaseUuid))
                return (null, null);

            var dbUuidObj = new PwUuid(Convert.FromBase64String(databaseUuid));
            var database = mainWindow.DocumentManager.GetOpenDatabases()
                               .FirstOrDefault(db => db.RootGroup.Uuid.Equals(dbUuidObj));
            if (database == null) return (null, null);

            var entryUuidObj = new PwUuid(Convert.FromBase64String(entryUuid));
            return (database.RootGroup.FindEntry(entryUuidObj, true), database);
        }

        private static List<PwGroup> CollectGroups(PwGroup root)
        {
            var list = new List<PwGroup> { root };
            foreach (var sub in root.GetGroups(false))
                list.AddRange(CollectGroups(sub));
            return list;
        }

        private static string UuidToString(PwUuid uuid) =>
            Convert.ToBase64String(uuid.UuidBytes);

        private static string ResolveDisplay(string value, PwEntry entry, PwDatabase db, bool resolve)
        {
            if (resolve && value.IndexOf('{') >= 0)
                return SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));
            return value;
        }

        private byte[] GetEntryIconBytes(PwEntry entry, PwDatabase db)
        {
            if (!entry.CustomIconUuid.Equals(PwUuid.Zero))
            {
                var customImage = db.GetCustomIcon(entry.CustomIconUuid, 24, 24);
                if (customImage != null)
                    return ImageToBytes(customImage);
            }

            return GetBuiltInIconCached((int)entry.IconId);
        }

        private byte[] GetBuiltInIconCached(int iconId)
        {
            if (!builtInIconCache.TryGetValue(iconId, out var bytes))
            {
                var image = mainWindow.ClientIcons.Images[iconId];
                bytes = ImageToBytes(image);
                builtInIconCache[iconId] = bytes;
            }
            return bytes;
        }

        private static byte[] ImageToBytes(Image image)
        {
            if (image == null) return null;
            using (var ms = new MemoryStream())
            using (var bmp = new Bitmap(image))
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        // ── Settings ──────────────────────────────────────────────────────────────

        private Settings LoadOrCreateDefaultSettings()
        {
            var configString = pluginHost.CustomConfig.GetString(nameof(FluentPassFinderPlugin));
            if (configString == null)
                return CreateDefaultSettings();

            try
            {
                return JsonConvert.DeserializeObject<Settings>(configString, jsonSerializerSettings);
            }
            catch
            {
                return CreateDefaultSettings();
            }
        }

        private Settings CreateDefaultSettings()
        {
            pluginHost.CustomConfig.SetString(nameof(FluentPassFinderPlugin),
                JsonConvert.SerializeObject(Settings.DefaultSettings, jsonSerializerSettings));
            return Settings.DefaultSettings;
        }

        private static PipeEnvelope Ack(string id, bool success = true, string error = null) =>
            new PipeEnvelope { Id = id, Success = success, Error = error };
    }
}
