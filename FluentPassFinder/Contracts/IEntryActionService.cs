using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    internal interface IEntryActionService
    {
        void RunAction(EntrySearchResult searchResult, string actionType);
        void RunAction(EntrySearchResult searchResult, IAction action);
        IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult, bool includeHiddenActions);
    }
}
