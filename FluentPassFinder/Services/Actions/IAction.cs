using FluentPassFinderContracts.Services;
using System.Windows.Input;

namespace FluentPassFinder.Services.Actions
{
    internal interface IAction : ICommand
    {
        void RunAction(EntrySearchResult searchResult);

        ActionType ActionType { get; }
    }
}
