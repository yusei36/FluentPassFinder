using FluentPassFinderContracts;
using System.Windows.Input;

namespace FluentPassFinder.Contracts
{
    public interface IAction : ICommand
    {
        void RunAction(EntrySearchResult searchResult);

        string ActionType { get; }

        int SortingIndex { get; }

        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService);
    }
}
