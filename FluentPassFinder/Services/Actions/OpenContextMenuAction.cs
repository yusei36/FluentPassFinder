using FluentPassFinder.Contracts;
using FluentPassFinder.ViewModels;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class OpenContextMenuAction : ActionBase
    {
        private readonly Lazy<SearchWindowViewModel> lazySearchWindowViewModel;

        public OpenContextMenuAction(Lazy<SearchWindowViewModel> lazySearchWindowViewModel)
        {
            this.lazySearchWindowViewModel = lazySearchWindowViewModel;
        }

        public override string ActionType => FluentPassFinderContracts.ActionType.OpenContextMenu.ToString();

        public override int SortingIndex => -1;

        public override void RunAction(EntrySearchResult searchResult)
        {
            lazySearchWindowViewModel.Value.IsContextMenuOpen = true;
        }
    }
}
