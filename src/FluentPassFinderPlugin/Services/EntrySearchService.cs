// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Contracts.Public.Ipc;
using KeePass.Util.Spr;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Searches the open databases and projects matching entries into <see cref="EntryDto"/>s.
    /// Matching honors the user's <see cref="SearchOptions"/> (which fields, tags, expiry, group
    /// search flags, reference resolution).
    /// </summary>
    internal class EntrySearchService
    {
        private readonly KeePassContext context;
        private readonly SettingsStore settings;
        private readonly EntryIconRenderer iconRenderer;

        public EntrySearchService(KeePassContext context, SettingsStore settings, EntryIconRenderer iconRenderer)
        {
            this.context = context;
            this.settings = settings;
            this.iconRenderer = iconRenderer;
        }

        public SearchEntriesResponse Search(SearchEntriesRequest req)
        {
            var entries = context.Invoke(() =>
            {
                var query = (req.Query ?? string.Empty).ToLowerInvariant();
                var searchOptions = settings.Current.Search;
                var searchTime = DateTime.Now;
                var results = new List<EntryDto>();

                foreach (var db in context.OpenDatabases)
                {
                    var allGroups = CollectGroups(db.RootGroup);
                    var includedGroups = searchOptions.ExcludeGroupsBySearchSetting
                        ? allGroups.Where(g => g.GetSearchingEnabledInherited()).ToList()
                        : allGroups;

                    var dbUuid = KeePassContext.UuidToString(db.RootGroup.Uuid);
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

        private static bool MatchesQuery(PwEntry entry, PwDatabase db, string query, SearchOptions opts)
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

        private static IEnumerable<string> GetSearchFieldNames(PwEntry entry, SearchOptions opts)
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
            var resolve = settings.Current.Search.ResolveFieldReferences;
            var dto = new EntryDto
            {
                Uuid     = KeePassContext.UuidToString(entry.Uuid),
                DatabaseUuid = dbUuid,
                Title    = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.TitleField),    entry, db, resolve),
                UserName = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UserNameField), entry, db, resolve),
                Url      = ResolveDisplay(entry.Strings.ReadSafe(PwDefs.UrlField),      entry, db, resolve),
                Expires  = entry.Expires,
                IsTemplate = IsTemplateEntry(entry, db),
                Tags     = entry.Tags.ToList(),
                Icon     = iconRenderer.GetEntryIconBytes(entry, db),
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

        private static List<PwGroup> CollectGroups(PwGroup root)
        {
            var list = new List<PwGroup> { root };
            foreach (var sub in root.GetGroups(false))
                list.AddRange(CollectGroups(sub));
            return list;
        }

        private static string ResolveDisplay(string value, PwEntry entry, PwDatabase db, bool resolve)
        {
            if (resolve && value.IndexOf('{') >= 0)
                return SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));
            return value;
        }

        private static bool IsTemplateEntry(PwEntry entry, PwDatabase db)
        {
            var tplGroupUuid = db.EntryTemplatesGroup;
            if (tplGroupUuid == null || tplGroupUuid.Equals(PwUuid.Zero)) return false;

            for (var group = entry.ParentGroup; group != null; group = group.ParentGroup)
                if (group.Uuid.Equals(tplGroupUuid)) return true;

            return false;
        }
    }
}
