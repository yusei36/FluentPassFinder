using KeePassLib;
using System.Collections.Generic;

namespace KeePassEntrySearcherContracts.Services
{
    public interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, SearchOptions searchOptions);
    }
}
