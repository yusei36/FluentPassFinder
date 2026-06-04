// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
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

        public override int DefaultSortingIndex => -1;
        public override string ActionType => Consts.OpenContextMenu;
        public override string DisplayName => "Open context menu for selected entry";
        public override string IconGlyph => Icons.Menu;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var vm = lazySearchWindowViewModel.Value;
            vm.IsContextMenuOpen = true;
        }
    }
}
