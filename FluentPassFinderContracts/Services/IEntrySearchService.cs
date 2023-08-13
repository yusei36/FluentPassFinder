using KeePassLib;
using System.Collections.Generic;

namespace FluentPassFinderContracts.Services
{
    public interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, SearchOptions searchOptions);
    }
}
