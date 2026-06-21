// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    /// <summary>
    /// Shown only for template entries (those in the database's template group). Opens the
    /// create-entry overlay with this template preselected.
    /// </summary>
    internal class CreateFromTemplateAction : ActionBase, IStaticAction
    {
        private readonly Lazy<SearchWindowViewModel> lazySearchWindowViewModel;

        public CreateFromTemplateAction(Lazy<SearchWindowViewModel> lazySearchWindowViewModel)
        {
            this.lazySearchWindowViewModel = lazySearchWindowViewModel;
        }

        public override int DefaultSortingIndex => 350;
        public override string ActionType => Consts.CreateFromTemplate;
        public override string DisplayName => "Create entry from this template";
        public override string IconGlyph => Icons.Add;

        public override bool CanRunAction(EntrySearchResult searchResult) =>
            searchResult?.Entry?.IsTemplate == true;

        public override void RunAction(EntrySearchResult searchResult)
        {
            lazySearchWindowViewModel.Value.OpenCreateEntry(searchResult.Entry.Uuid);
        }
    }
}
