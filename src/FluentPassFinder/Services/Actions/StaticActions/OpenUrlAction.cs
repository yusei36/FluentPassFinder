// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class OpenUrlAction : ActionBase, IStaticAction
    {
        public override string DisplayName => "Open URL of selected entry";
        public override string ActionType => Consts.OpenUrl;
        public override int DefaultSortingIndex => 200;
        public override string IconGlyph => Icons.Globe;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.OpenEntryUrl(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid);
        }
    }
}
