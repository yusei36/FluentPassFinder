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

        public PipeResponse Handle(PipeRequest request)
        {
            try
            {
                switch (request.Type)
                {
                    case PipeRequestTypes.SearchEntries:       return HandleSearchEntries(request);
                    case PipeRequestTypes.GetPlaceholderValue: return HandleGetPlaceholderValue(request);
                    case PipeRequestTypes.GetStringFromCustomConfig: return HandleGetStringFromCustomConfig(request);
                    case PipeRequestTypes.GetSettings:         return HandleGetSettings(request);
                    case PipeRequestTypes.IsAnyDatabaseOpen:   return HandleIsAnyDatabaseOpen(request);
                    case PipeRequestTypes.CopyField:           return HandleCopyField(request);
                    case PipeRequestTypes.CopyToClipboard:     return HandleCopyToClipboard(request);
                    case PipeRequestTypes.AutoTypeField:       return HandleAutoTypeField(request);
                    case PipeRequestTypes.PerformAutoType:     return HandlePerformAutoType(request);
                    case PipeRequestTypes.OpenEntryUrl:        return HandleOpenEntryUrl(request);
                    case PipeRequestTypes.SelectEntry:         return HandleSelectEntry(request);
                    default:
                        return Error(request.Id, $"Unknown request type: {request.Type}");
                }
            }
            catch (Exception ex)
            {
                return Error(request.Id, ex.Message);
            }
        }

        // ── Search ────────────────────────────────────────────────────────────────

        private PipeResponse HandleSearchEntries(PipeRequest request)
        {
            var entries = pluginHostDispatcher.Invoke(() =>
            {
                var query = (request.Query ?? string.Empty).ToLowerInvariant();
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

            return new PipeResponse { Id = request.Id, Success = true, Entries = entries.ToArray() };
        }

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
                Uuid = UuidToString(entry.Uuid),
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

        // ── Actions ───────────────────────────────────────────────────────────────

        private PipeResponse HandleGetPlaceholderValue(PipeRequest request)
        {
            var value = pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return request.Placeholder;
                var flags = (request.ResolveAll == true) ? SprCompileFlags.All : SprCompileFlags.Deref;
                return SprEngine.Compile(request.Placeholder, new SprContext(entry, db, flags, true, false));
            });
            return new PipeResponse { Id = request.Id, Success = true, StringValue = value };
        }

        private PipeResponse HandleGetStringFromCustomConfig(PipeRequest request)
        {
            var value = pluginHostDispatcher.Invoke(() =>
                pluginHost.CustomConfig.GetString(request.ConfigId, request.DefaultValue));
            return new PipeResponse { Id = request.Id, Success = true, StringValue = value };
        }

        private PipeResponse HandleGetSettings(PipeRequest request)
        {
            return new PipeResponse { Id = request.Id, Success = true, Settings = settings };
        }

        private PipeResponse HandleIsAnyDatabaseOpen(PipeRequest request)
        {
            var isOpen = pluginHostDispatcher.Invoke(() =>
                mainWindow.DocumentManager.GetOpenDatabases().Any());
            return new PipeResponse { Id = request.Id, Success = true, BoolValue = isOpen };
        }

        private PipeResponse HandleCopyField(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(request.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ok(request.Id);
        }

        private PipeResponse HandleCopyToClipboard(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return;

                if (ClipboardUtil.Copy(request.Value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ok(request.Id);
        }

        private PipeResponse HandleAutoTypeField(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(request.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                AutoType.PerformIntoCurrentWindow(entry, db, value + "{ENTER}");
            });
            return Ok(request.Id);
        }

        private PipeResponse HandlePerformAutoType(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return;
                AutoType.PerformIntoCurrentWindow(entry, db, request.Sequence);
            });
            return Ok(request.Id);
        }

        private PipeResponse HandleOpenEntryUrl(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, _) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry != null) WinUtil.OpenEntryUrl(entry);
            });
            return Ok(request.Id);
        }

        private PipeResponse HandleSelectEntry(PipeRequest request)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                var (entry, db) = ResolveEntry(request.EntryUuid, request.DatabaseUuid);
                if (entry == null) return;

                mainWindow.UpdateUI(false, mainWindow.DocumentManager.FindDocument(db), true, entry.ParentGroup, true, null, false);
                mainWindow.SelectEntries(new PwObjectList<PwEntry> { entry }, true, true);
                mainWindow.EnsureVisibleEntry(entry.Uuid);
                mainWindow.UpdateUI(false, null, false, null, false, null, false);
                mainWindow.EnsureVisibleForegroundWindow(true, true);
            });
            return Ok(request.Id);
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
            var entry = database.RootGroup.FindEntry(entryUuidObj, true);
            return (entry, database);
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
            {
                // Clone via Bitmap to avoid GDI+ stream-dependency quirk
                using (var bmp = new Bitmap(image))
                {
                    bmp.Save(ms, ImageFormat.Png);
                }
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

        private static PipeResponse Ok(string id) =>
            new PipeResponse { Id = id, Success = true };

        private static PipeResponse Error(string id, string message) =>
            new PipeResponse { Id = id, Success = false, Error = message };
    }
}
