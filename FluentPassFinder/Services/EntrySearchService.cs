using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services
{
    class EntrySearchService : IEntrySearchService
    {
        public IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, SearchOptions searchOptions)
        {
            searchQuery = searchQuery.ToLower();
            foreach (PwDatabase db in databases)
            {
                var allEntriesInDb = db.RootGroup.GetEntries(true);
                foreach (var entry in allEntriesInDb)
                {
                    var fieldNamesToSearch = GetFieldNamesToSearch(entry, searchOptions);
                    foreach (var fieldName in fieldNamesToSearch)
                    {
                        var fieldValue = entry.Strings.ReadSafe(fieldName);
                        if (fieldValue.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                        {
                            yield return new EntrySearchResult { Entry = entry, Database = db };
                            break;
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetFieldNamesToSearch(PwEntry entry, SearchOptions searchOptions)
        {
            var fields = new List<string>();
            if (searchOptions.IncludeTitleFiled)
            {
                fields.Add(PwDefs.TitleField);
            }
            if (searchOptions.IncludeUserNameField)
            {
                fields.Add(PwDefs.UserNameField);
            }
            if (searchOptions.IncludePasswordField)
            {
                fields.Add(PwDefs.PasswordField);
            }
            if (searchOptions.IncludeNotesField)
            {
                fields.Add(PwDefs.NotesField);
            }
            if (searchOptions.IncludeUrlField)
            {
                fields.Add(PwDefs.UrlField);
            }

            if (searchOptions.IncludeCustomFields)
            {
                var customFields = entry.Strings.Where(fieldN => !PwDefs.IsStandardField(fieldN.Key));
                if (searchOptions.IncludeProtectedCustomFields)
                {
                    fields.AddRange(customFields.Select(field => field.Key));
                }
                else
                {
                    foreach (var customField in customFields)
                    {
                        if (!customField.Value.IsProtected)
                        {
                            fields.Add(customField.Key);
                        }
                    }
                }
            }

            return fields;
        }
    }
}
