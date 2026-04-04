using System.Collections.Generic;

namespace FluentPassFinder.Contracts
{
    internal interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(string query);
    }
}
