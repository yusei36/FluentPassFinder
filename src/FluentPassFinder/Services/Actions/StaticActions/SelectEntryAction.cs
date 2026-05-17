// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class SelectEntryAction : ActionBase, IStaticAction
    {
        public override string DisplayName => "Select entry in main window";
        public override string ActionType => Consts.SelectEntry;
        public override int DefaultSortingIndex => 300;
        public override string IconPath => Icons.Checkmark;

        public override void RunAction(EntrySearchResult searchResult)
        {
            pluginProxy.SelectEntry(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid);
        }
    }
}
