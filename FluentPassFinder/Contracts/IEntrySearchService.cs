using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    public interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(IEnumerable<PwDatabase> databases, string searchQuery, Settings settings);
    }
}
