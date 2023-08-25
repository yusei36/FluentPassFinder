using FluentPassFinder.Contracts.Public;
using System.Windows.Input;

namespace FluentPassFinder.Contracts
{
    internal interface IAction : ICommand
    {
        string ActionType { get; }
        int SortingIndex { get; }
        string DisplayName { get; }

        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService);
        void RunAction(EntrySearchResult searchResult);
    }
}
