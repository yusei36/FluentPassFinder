// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Contracts.Public.Ipc;
using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Lists the entry templates the app can create from and creates new entries from a chosen
    /// template plus field values. Supports both KeePass core templates (any entry in the
    /// database's template group, cloned field-by-field) and the third-party KPEntryTemplates
    /// plugin format (templates carrying typed "_etm_*" field definitions).
    /// </summary>
    internal class EntryCreationService
    {
        // KPEntryTemplates plugin field markers (third-party "_etm_*" convention).
        // KeePass core has no typed templates; it just clones an entry from the
        // template group. We support both (see GetTemplates).
        private const string EtmPrefix = "_etm_";
        private const string EtmTemplateMarker = "_etm_template";
        private const string EtmTitlePrefix = "_etm_title_";
        private const string EtmTypePrefix = "_etm_type_";
        private const string EtmPositionPrefix = "_etm_position_";
        private const string EtmOptionsPrefix = "_etm_options_";
        private static readonly string[] EmptyOptions = new string[0];

        private readonly KeePassContext context;
        private readonly EntryIconRenderer iconRenderer;
        private readonly SettingsStore settings;

        public EntryCreationService(KeePassContext context, EntryIconRenderer iconRenderer, SettingsStore settings)
        {
            this.context = context;
            this.iconRenderer = iconRenderer;
            this.settings = settings;
        }

        public GetGroupsResponse GetGroups()
        {
            var groups = context.Invoke(() =>
            {
                var list = new List<GroupDto>();
                var db = context.ActiveDatabase;
                if (db == null || !db.IsOpen) return list;

                var root = db.RootGroup;
                // Root group keeps its own name as label (tagged so it stands out); descendants
                // are pathed relative to it (the root group name is omitted from their paths).
                list.Add(new GroupDto
                {
                    Uuid = root.Uuid.ToHexString(),
                    Name = root.Name,
                    Path = root.Name + " (Root)",
                    Icon = iconRenderer.GetGroupIconBytes(root, db),
                });

                foreach (var sub in root.GetGroups(false))
                    CollectGroups(sub, string.Empty, list, db);

                return list;
            });

            return new GetGroupsResponse { Success = true, Groups = groups.ToArray() };
        }

        private void CollectGroups(PwGroup group, string parentPath, List<GroupDto> list, PwDatabase db)
        {
            // Skip the recycle bin (and its descendants) as a target.
            if (db.RecycleBinEnabled && !db.RecycleBinUuid.Equals(PwUuid.Zero) && group.Uuid.Equals(db.RecycleBinUuid))
                return;

            var path = string.IsNullOrEmpty(parentPath) ? group.Name : parentPath + " / " + group.Name;
            list.Add(new GroupDto
            {
                Uuid = group.Uuid.ToHexString(),
                Name = group.Name,
                Path = path,
                Icon = iconRenderer.GetGroupIconBytes(group, db),
            });

            foreach (var sub in group.GetGroups(false))
                CollectGroups(sub, path, list, db);
        }

        public GetTemplatesResponse GetTemplates()
        {
            var templates = context.Invoke(() =>
            {
                var list = new List<TemplateDto>();
                var db = context.ActiveDatabase;
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
                Uuid = KeePassContext.UuidToString(entry.Uuid),
                Name = entry.Strings.ReadSafe(PwDefs.TitleField),
                Icon = iconRenderer.GetEntryIconBytes(entry, db),
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
        private static TemplateFieldDto MakeCustomField(KeyValuePair<string, ProtectedString> kv)
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

        public CreateEntryResponse CreateEntry(CreateEntryRequest req)
        {
            var createdUuid = context.Invoke(() =>
            {
                var db = context.ActiveDatabase;
                if (db == null || !db.IsOpen) return null;

                var parentGroup = ResolveTargetGroup(db, req.TargetGroupUuid);
                if (parentGroup == null) return null;

                var entry = new PwEntry(true, true);

                var template = string.IsNullOrEmpty(req.TemplateUuid) ? null : ResolveTemplate(db, req.TemplateUuid);
                if (template != null)
                {
                    // Use the same icon as the template entry.
                    entry.IconId = template.IconId;
                    entry.CustomIconUuid = template.CustomIconUuid;
                }

                // The template (parsed the same way the app's create form was built) is the
                // source of truth for which fields are protected, so derive it here rather than
                // trusting a client-supplied list.
                var protectedNames = BuildProtectedFieldSet(template);

                if (req.Fields != null)
                {
                    foreach (var kv in req.Fields)
                    {
                        if (kv.Value == null) continue;
                        // Honor the template's protected set and the database's default
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
                context.MainWindow.UpdateUI(false, null, true, null, true, null, /*bSetModified:*/ true);
                return KeePassContext.UuidToString(entry.Uuid);
            });

            return createdUuid == null
                ? new CreateEntryResponse { Success = false, Error = "No open database to create the entry in." }
                : new CreateEntryResponse { Success = true, CreatedEntryUuid = createdUuid };
        }

        /// <summary>Generates a password using KeePass's configured auto-generation profile, for the create form.</summary>
        public GeneratePasswordResponse GeneratePassword()
        {
            var password = context.Invoke(() =>
            {
                var profile = KeePass.Program.Config?.PasswordGenerator?.AutoGeneratedPasswordsProfile
                              ?? new PwProfile();
                var error = PwGenerator.Generate(out var generated, profile, null, KeePass.Program.PwGeneratorPool);
                return error == PwgError.Success && generated != null ? generated.ReadString() : string.Empty;
            });

            return new GeneratePasswordResponse { Success = true, Password = password };
        }

        private static PwEntry ResolveTemplate(PwDatabase db, string base64Uuid)
        {
            var uuid = new PwUuid(Convert.FromBase64String(base64Uuid));
            return db.RootGroup.FindEntry(uuid, true);
        }

        // The group new entries are saved into, from settings. If the configured group is
        // missing (the default group on first use, or a deleted custom group), it is created as
        // "New entries" with the configured UUID under the root. Falls back to the root group
        // when the setting is empty or unparseable.
        private PwGroup ResolveTargetGroup(PwDatabase db, string requestedUuidHex)
        {
            // Per-create override (from the create form) wins; otherwise the configured default.
            var uuidHex = string.IsNullOrEmpty(requestedUuidHex)
                ? settings.Current.EntryCreation?.NewEntryGroupUuid
                : requestedUuidHex;
            if (string.IsNullOrEmpty(uuidHex))
                return db.RootGroup;

            byte[] uuidBytes;
            try { uuidBytes = MemUtil.HexStringToByteArray(uuidHex); }
            catch { return db.RootGroup; }
            if (uuidBytes == null || uuidBytes.Length != 16)
                return db.RootGroup;

            var uuid = new PwUuid(uuidBytes);
            var existing = db.RootGroup.FindGroup(uuid, true);
            if (existing != null) return existing;

            var created = new PwGroup(false, true, Consts.DefaultNewEntryGroupName, PwIcon.Folder) { Uuid = uuid };
            db.RootGroup.AddGroup(created, true);
            return created;
        }
    }
}
