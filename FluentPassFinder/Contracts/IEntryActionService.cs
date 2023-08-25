using FluentPassFinderContracts;

namespace FluentPassFinder.Contracts
{
    public interface IEntryActionService
    {
        void RunAction(EntrySearchResult searchResult, string actionType);
        void RunAction(EntrySearchResult searchResult, IAction action);
        IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult, bool includeHiddenActions);
    }
}
