using FluentPassFinder.Contracts;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class OpenContextMenuAction : ActionBase, IStaticAction
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
