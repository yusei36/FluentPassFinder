using FluentPassFinderContracts.Services;

namespace FluentPassFinder.Services.Actions
{
    internal interface IAction
    {
        void RunAction(EntrySearchResult searchResult);

        ActionType ActionType { get; }
    }
}
