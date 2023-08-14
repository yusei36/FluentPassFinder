using FluentPassFinderContracts;

namespace FluentPassFinder.Contracts
{
    public interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, SearchOptions searchOptions);
    }
}
