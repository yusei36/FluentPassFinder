// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Contracts.Public.Ipc;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace FluentPassFinder.Ipc
{
    internal class PluginRequestHandler
    {
        private const string ConfigKey = "FluentPassFinder";

        private readonly IPluginHost pluginHost;
        private readonly MainForm mainWindow;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Dictionary<int, byte[]> builtInIconCache = new Dictionary<int, byte[]>();

        private Settings settings;

        public PluginRequestHandler(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            mainWindow = pluginHost.MainWindow;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            // Replace default-initialized collections instead of appending to them,
            // otherwise default entries (e.g. ExcludeFields) duplicate on every load/save round-trip.
            jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            settings = LoadOrCreateDefaultSettings();
        }

        public PipeResponse Handle(PipeRequest request)
        {
            try
            {
                switch (request)
                {
                    case SearchEntriesRequest req:             return HandleSearchEntries(req);
                    case HasTotpRequest req:                   return HandleHasTotp(req);
                    case GetSettingsRequest _:                 return new GetSettingsResponse { Success = true, Settings = settings };
                    case IsAnyDatabaseOpenRequest req:         return HandleIsAnyDatabaseOpen(req);
                    case CopyFieldRequest req:                 return HandleCopyField(req);
                    case CopyToClipboardRequest req:           return HandleCopyToClipboard(req);
                    case AutoTypeFieldRequest req:             return HandleAutoTypeField(req);
                    case PerformAutoTypeRequest req:           return HandlePerformAutoType(req);
                    case OpenEntryUrlRequest req:              return HandleOpenEntryUrl(req);
                    case SelectEntryRequest req:               return HandleSelectEntry(req);
                    case SaveSettingsRequest req:              return HandleSaveSettings(req);
                    case GetTemplatesRequest req:              return HandleGetTemplates(req);
                    case CreateEntryRequest req:               return HandleCreateEntry(req);
                    case GeneratePasswordRequest req:          return HandleGeneratePassword(req);
                    default:                                   return Ack(success: false, error: $"Unknown request type: {request.Type}");
                }
            }
            catch (Exception ex)
            {
                return Ack(success: false, error: ex.Message);
            }
        }

        private PipeResponse HandleSearchEntries(SearchEntriesRequest req)
        {
            var entries = InvokeOnUiThread(() =>
            {
                var query = (req.Query ?? string.Empty).ToLowerInvariant();
                var searchOptions = settings.Search;
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

            return new SearchEntriesResponse { Success = true, Entries = entries.ToArray() };
        }

        private PipeResponse HandleHasTotp(HasTotpRequest req)
        {
            var hasTotp = InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return false;
                var value = SprEngine.Compile(req.Placeholder, new SprContext(entry, db, SprCompileFlags.All, true, false));
                return !string.IsNullOrEmpty(value) && value != req.Placeholder;
            });
            return new HasTotpResponse { Success = true, HasTotp = hasTotp };
        }

        private PipeResponse HandleIsAnyDatabaseOpen(IsAnyDatabaseOpenRequest req)
        {
            var isOpen = InvokeOnUiThread(() =>
                mainWindow.DocumentManager.GetOpenDatabases().Any());
            return new IsAnyDatabaseOpenResponse { Success = true, IsOpen = isOpen };
        }

        private PipeResponse HandleCopyField(CopyFieldRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ack();
        }

        private PipeResponse HandleCopyToClipboard(CopyToClipboardRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = req.Value;
                if (!string.IsNullOrEmpty(value) && value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.All, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    mainWindow.StartClipboardCountdown();
            });
            return Ack();
        }

        private PipeResponse HandleAutoTypeField(AutoTypeFieldRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                AutoType.PerformIntoCurrentWindow(entry, db, value + "{ENTER}");
            });
            return Ack();
        }

        private PipeResponse HandlePerformAutoType(PerformAutoTypeRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null)
                    AutoType.PerformIntoCurrentWindow(entry, db, req.Sequence);
            });
            return Ack();
        }

        private PipeResponse HandleOpenEntryUrl(OpenEntryUrlRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, _) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null) WinUtil.OpenEntryUrl(entry);
            });
            return Ack();
        }

        private PipeResponse HandleSelectEntry(SelectEntryRequest req)
        {
            InvokeOnUiThread(() =>
            {
                var (entry, db) = ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                mainWindow.UpdateUI(false, mainWindow.DocumentManager.FindDocument(db), true, entry.ParentGroup, true, null, false);
                mainWindow.SelectEntries(new PwObjectList<PwEntry> { entry }, true, true);
                mainWindow.EnsureVisibleEntry(entry.Uuid);
                mainWindow.UpdateUI(false, null, false, null, false, null, false);
                mainWindow.EnsureVisibleForegroundWindow(true, true);
            });
            return Ack();
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
            var resolve = settings.Search.ResolveFieldReferences;
            var dto = new EntryDto
            {
                Uuid     = UuidToString(entry.Uuid),
                DatabaseUuid = dbUuid,
                Title    = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.TitleField),    entry, db, resolve),
                UserName = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UserNameField), entry, db, resolve),
                Url      = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UrlField),      entry, db, resolve),
                Expires  = entry.Expires,
                IsTemplate = IsTemplateEntry(entry, db),
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

        // --- Entry creation -------------------------------------------------

        // KPEntryTemplates plugin field markers (third-party "_etm_*" convention).
        // KeePass core has no typed templates; it just clones an entry from the
        // template group. We support both (see HandleGetTemplates).
        private const string EtmPrefix = "_etm_";
        private const string EtmTemplateMarker = "_etm_template";
        private const string EtmTitlePrefix = "_etm_title_";
        private const string EtmTypePrefix = "_etm_type_";
        private const string EtmPositionPrefix = "_etm_position_";
        private const string EtmOptionsPrefix = "_etm_options_";
        private static readonly string[] EmptyOptions = new string[0];

        private PipeResponse HandleGetTemplates(GetTemplatesRequest req)
        {
            var templates = InvokeOnUiThread(() =>
            {
                var list = new List<TemplateDto>();
                var db = pluginHost.Database;
                if (db == null || !db.IsOpen) return list;

                var tplGroupUuid = db.EntryTemplatesGroup;
                if (tplGroupUuid == null || tplGroupUuid.Equals(PwUuid.Zero)) return list;

                var group = db.RootGroup.FindGroup(tplGroupUuid, true);
                if (group == null) return list;

                foreach (var entry in group.GetEntries(true))
                    list.Add(BuildTemplateDto(entry, db));

                return list;
            });

            return new GetTemplatesResponse { Success = true, Templates = templates.ToArray() };
        }

        private TemplateDto BuildTemplateDto(PwEntry entry, PwDatabase db)
        {
            var isEtmTemplate = entry.Strings.Get(EtmTemplateMarker) != null;
            return new TemplateDto
            {
                Uuid = UuidToString(entry.Uuid),
                Name = entry.Strings.ReadSafe(PwDefs.TitleField),
                Icon = GetEntryIconBytes(entry, db),
                // Everything except Title, in display order. The app appends any standard
                // fields the template omits, so it always offers UserName/Password/URL/Notes.
                Fields = (isEtmTemplate ? ParseEtmFields(entry) : ParseClonedFields(entry)).ToArray(),
            };
        }

        private static HashSet<string> BuildProtectedFieldSet(PwEntry template)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (template == null) return set;

            var isEtmTemplate = template.Strings.Get(EtmTemplateMarker) != null;
            var fields = isEtmTemplate ? ParseEtmFields(template) : ParseClonedFields(template);
            foreach (var field in fields)
            {
                if (field.IsProtected)
                    set.Add(field.FieldName);
            }

            return set;
        }

        // Plain KeePass-core template: standard fields first (no positions exist), then custom clones.
        private static IEnumerable<TemplateFieldDto> ParseClonedFields(PwEntry entry)
        {
            yield return CloneField(entry, PwDefs.UserNameField, "User name");
            yield return CloneField(entry, PwDefs.PasswordField, "Password");
            yield return CloneField(entry, PwDefs.UrlField, "URL");
            yield return CloneField(entry, PwDefs.NotesField, "Notes");

            foreach (var kv in entry.Strings)
            {
                if (PwDefs.IsStandardField(kv.Key)) continue;
                if (kv.Key.StartsWith(EtmPrefix, StringComparison.Ordinal)) continue;

                yield return MakeCustomField(kv);
            }
        }

        // A plain custom string copied verbatim (value and protection) from the template.
        private static TemplateFieldDto MakeCustomField(System.Collections.Generic.KeyValuePair<string, ProtectedString> kv)
        {
            var prot = kv.Value.IsProtected;
            return new TemplateFieldDto
            {
                FieldName = kv.Key,
                Title = kv.Key,
                Type = prot ? TemplateFieldType.ProtectedText : TemplateFieldType.Text,
                IsProtected = prot,
                DefaultValue = kv.Value.ReadString(),
                Options = EmptyOptions,
                Lines = 1,
            };
        }

        private static TemplateFieldDto CloneField(PwEntry entry, string fieldName, string title)
        {
            var str = entry.Strings.Get(fieldName);
            var prot = fieldName == PwDefs.PasswordField || (str != null && str.IsProtected);
            var isPassword = fieldName == PwDefs.PasswordField;
            return new TemplateFieldDto
            {
                FieldName = fieldName,
                Title = title,
                Type = fieldName == PwDefs.NotesField
                    ? TemplateFieldType.MultiLine
                    : (prot ? TemplateFieldType.ProtectedText : TemplateFieldType.Text),
                IsProtected = prot,
                // Carry the template's value over, except the password (generate a fresh one).
                DefaultValue = isPassword ? string.Empty : (str != null ? str.ReadString() : string.Empty),
                Options = EmptyOptions,
                Lines = 1,
            };
        }

        // KPEntryTemplates template: parse the "_etm_*" field definitions, ordered by position.
        // Standard fields (except Title) are kept inline so their position is honored.
        private static IEnumerable<TemplateFieldDto> ParseEtmFields(PwEntry entry)
        {
            var fields = new List<KeyValuePair<int, TemplateFieldDto>>();
            var defined = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in entry.Strings)
            {
                if (!kv.Key.StartsWith(EtmTitlePrefix, StringComparison.Ordinal)) continue;

                var fieldName = kv.Key.Substring(EtmTitlePrefix.Length);
                if (string.IsNullOrEmpty(fieldName) || fieldName[0] == '@') continue; // built-in mappings
                if (string.Equals(fieldName, PwDefs.TitleField, StringComparison.Ordinal)) continue; // Title is the dedicated top field
                defined.Add(fieldName);

                var title = kv.Value.ReadString();
                var typeStr = entry.Strings.ReadSafe(EtmTypePrefix + fieldName);
                var optionsStr = entry.Strings.ReadSafe(EtmOptionsPrefix + fieldName);
                int.TryParse(entry.Strings.ReadSafe(EtmPositionPrefix + fieldName), out var position);

                var mapped = MapEtmType(typeStr, optionsStr);
                // Never seed a password default from the template.
                var isPassword = string.Equals(fieldName, PwDefs.PasswordField, StringComparison.Ordinal);
                var dto = new TemplateFieldDto
                {
                    FieldName = fieldName,
                    Title = string.IsNullOrEmpty(title) ? fieldName : title,
                    Type = mapped.Type,
                    IsProtected = mapped.IsProtected,
                    Options = mapped.Options,
                    Lines = mapped.Lines,
                    // Copy the template's field value (protected ones included), except the
                    // password (generate a fresh one) and dividers (no value).
                    DefaultValue = isPassword || mapped.Type == TemplateFieldType.Divider
                        ? string.Empty
                        : entry.Strings.ReadSafe(fieldName),
                };
                fields.Add(new KeyValuePair<int, TemplateFieldDto>(position, dto));
            }

            var ordered = fields.OrderBy(f => f.Key).Select(f => f.Value).ToList();

            // Also copy any plain custom strings the template carries that aren't part of
            // its _etm_ definition (and aren't standard fields or _etm_ markers).
            foreach (var kv in entry.Strings)
            {
                if (PwDefs.IsStandardField(kv.Key)) continue;
                if (kv.Key.StartsWith(EtmPrefix, StringComparison.Ordinal)) continue;
                if (defined.Contains(kv.Key)) continue;

                ordered.Add(MakeCustomField(kv));
            }

            return ordered;
        }

        private static (TemplateFieldType Type, bool IsProtected, int Lines, string[] Options) MapEtmType(string typeStr, string optionsStr)
        {
            typeStr = typeStr ?? string.Empty;
            switch (typeStr)
            {
                case "Checkbox":         return (TemplateFieldType.Checkbox, false, 1, EmptyOptions);
                case "Listbox":          return (TemplateFieldType.ListBox, false, 1, SplitOptions(optionsStr));
                case "Date":             return (TemplateFieldType.Date, false, 1, EmptyOptions);
                case "Time":             return (TemplateFieldType.Time, false, 1, EmptyOptions);
                case "Date Time":        return (TemplateFieldType.DateTime, false, 1, EmptyOptions);
                case "Divider":          return (TemplateFieldType.Divider, false, 1, EmptyOptions);
                case "RichTextbox":      return (TemplateFieldType.MultiLine, false, LinesFromOptions(optionsStr), EmptyOptions);
                case "Protected Inline": return (TemplateFieldType.ProtectedText, true, 1, EmptyOptions);
                case "Protected Popout": return (TemplateFieldType.ProtectedText, true, 1, EmptyOptions);
                case "Popout":           return (TemplateFieldType.Text, false, 1, EmptyOptions);
                case "Inline URL":       return (TemplateFieldType.Text, false, 1, EmptyOptions);
                case "Inline":
                default:
                    var lines = LinesFromOptions(optionsStr);
                    return (lines > 1 ? TemplateFieldType.MultiLine : TemplateFieldType.Text, false, lines, EmptyOptions);
            }
        }

        private static string[] SplitOptions(string options) =>
            string.IsNullOrEmpty(options)
                ? EmptyOptions
                : options.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray();

        private static int LinesFromOptions(string options)
        {
            if (string.IsNullOrEmpty(options)) return 1;
            return int.TryParse(options, out var n) && n >= 1 && n <= 100 ? n : 1;
        }

        private bool IsTemplateEntry(PwEntry entry, PwDatabase db)
        {
            var tplGroupUuid = db.EntryTemplatesGroup;
            if (tplGroupUuid == null || tplGroupUuid.Equals(PwUuid.Zero)) return false;

            for (var group = entry.ParentGroup; group != null; group = group.ParentGroup)
                if (group.Uuid.Equals(tplGroupUuid)) return true;

            return false;
        }

        private PipeResponse HandleCreateEntry(CreateEntryRequest req)
        {
            var createdUuid = InvokeOnUiThread(() =>
            {
                var db = pluginHost.Database;
                if (db == null || !db.IsOpen) return null;

                var parentGroup = db.RootGroup;
                if (parentGroup == null) return null;

                var entry = new PwEntry(true, true);

                var template = string.IsNullOrEmpty(req.TemplateUuid) ? null : ResolveTemplate(db, req.TemplateUuid);
                if (template != null)
                {
                    // Use the same icon as the template entry.
                    entry.IconId = template.IconId;
                    entry.CustomIconUuid = template.CustomIconUuid;
                }

                var protectedNames = BuildProtectedFieldSet(template);

                if (req.Fields != null)
                {
                    foreach (var kv in req.Fields)
                    {
                        if (kv.Value == null) continue;
                        // Honor the request's protected set and the database's default
                        // memory protection for standard fields (e.g. Password).
                        var prot = protectedNames.Contains(kv.Key) || db.MemoryProtection.GetProtection(kv.Key);
                        entry.Strings.Set(kv.Key, new ProtectedString(prot, kv.Value));
                    }
                }

                // Link to a KPEntryTemplates template so KeePass shows the typed child view.
                if (template != null && template.Strings.Get(EtmTemplateMarker) != null)
                    entry.Strings.Set(Consts.TemplateUuidField,
                        new ProtectedString(false, template.Uuid.ToHexString()));

                parentGroup.AddEntry(entry, true);
                mainWindow.UpdateUI(false, null, true, null, true, null, /*bSetModified:*/ true);
                return UuidToString(entry.Uuid);
            });

            return createdUuid == null
                ? new CreateEntryResponse { Success = false, Error = "No open database to create the entry in." }
                : new CreateEntryResponse { Success = true, CreatedEntryUuid = createdUuid };
        }

        private static PwEntry ResolveTemplate(PwDatabase db, string base64Uuid)
        {
            var uuid = new PwUuid(Convert.FromBase64String(base64Uuid));
            return db.RootGroup.FindEntry(uuid, true);
        }

        private PipeResponse HandleGeneratePassword(GeneratePasswordRequest req)
        {
            var password = InvokeOnUiThread(() =>
            {
                var profile = KeePass.Program.Config?.PasswordGenerator?.AutoGeneratedPasswordsProfile
                              ?? new PwProfile();
                var error = PwGenerator.Generate(out var generated, profile, null, KeePass.Program.PwGeneratorPool);
                return error == PwgError.Success && generated != null ? generated.ReadString() : string.Empty;
            });

            return new GeneratePasswordResponse { Success = true, Password = password };
        }

        private PipeResponse HandleSaveSettings(SaveSettingsRequest req)
        {
            settings = req.Settings;
            InvokeOnUiThread(() =>
                pluginHost.CustomConfig.SetString(ConfigKey,
                    JsonConvert.SerializeObject(settings, jsonSerializerSettings)));
            return Ack();
        }

        private Settings LoadOrCreateDefaultSettings()
        {
            var configString = pluginHost.CustomConfig.GetString(ConfigKey);
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
            var defaults = Settings.CreateDefault();
            pluginHost.CustomConfig.SetString(ConfigKey,
                JsonConvert.SerializeObject(defaults, jsonSerializerSettings));
            return defaults;
        }

        private void InvokeOnUiThread(Action action) =>
            mainWindow.Invoke(action);

        private T InvokeOnUiThread<T>(Func<T> func) =>
            (T)mainWindow.Invoke(func);

        private static PipeResponse Ack(bool success = true, string error = null) =>
            new PipeResponse { Success = success, Error = error };
    }
}
