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

        public override ActionType ActionType => ActionType.OpenContextMenu;

        public override void RunAction(EntrySearchResult searchResult)
        {
            lazySearchWindowViewModel.Value.IsContextMenuOpen = true;
        }
    }
}
