// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class AutoTypeEntryAction : ActionBase, IStaticAction
    {
        public override int DefaultSortingIndex => 0;
        public override string ActionType => Consts.AutoType;
        public override string DisplayName => "Auto type selected entry";
        public override string IconGlyph => Icons.Keyboard;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.CloseThen(() =>
                pluginProxy.PerformAutoType(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid));
        }
    }
}
