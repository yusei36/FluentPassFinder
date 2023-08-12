using KeePassEntrySearcherContracts.Services;
using KeePassLib;

namespace KeePassEntrySearcherWpf.Services
{
    class EntrySearchService : IEntrySearchService
    {
        public IEnumerable<PwEntry> GetPwEntries(IEnumerable<PwDatabase> pwDatabases, string searchQuery)
        {
            searchQuery = searchQuery.ToLower();

            if (pwDatabases.Any())
            {
                var allEntries = pwDatabases.SelectMany(db => db.RootGroup.GetEntries(true));
                foreach (var entry in allEntries)
                {
                    var title = entry.Strings.ReadSafe(PwDefs.TitleField);
                    if (title.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase))
                    { 
                        yield return entry;
                    }
                }
            }
        }
    }
}
