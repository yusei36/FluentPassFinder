using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    internal interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, Settings settings);
    }
}
