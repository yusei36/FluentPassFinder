using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;
using System.IO;

namespace KeePassEntrySearcherWpf.Services
{
    class EntrySearchService : IEntrySearchService
    {
        public IEnumerable<PwEntry> GetPwEntries(IEnumerable<PwDatabase> pwDatabases, string searchQuery, SearchOptions searchOptions)
        {
            searchQuery = searchQuery.ToLower();

            if (pwDatabases.Any())
            {
                var allEntries = pwDatabases.SelectMany(db => db.RootGroup.GetEntries(true));
                foreach (var entry in allEntries)
                {
                    var fieldNamesToSearch = GetFieldNamesToSearch(entry, searchOptions);
                    foreach (var fieldName in fieldNamesToSearch)
                    {
                        var fieldValue = entry.Strings.ReadSafe(fieldName);
                        if (fieldValue.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                        {
                            yield return entry;
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
