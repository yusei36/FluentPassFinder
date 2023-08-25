using FluentPassFinder.Contracts.Public;
using System.Windows.Input;

namespace FluentPassFinder.Contracts
{
    public interface IAction : ICommand
    {
        string ActionType { get; }
        int SortingIndex { get; }
        string DisplayName { get; }

        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService);
        void RunAction(EntrySearchResult searchResult);
    }
}
