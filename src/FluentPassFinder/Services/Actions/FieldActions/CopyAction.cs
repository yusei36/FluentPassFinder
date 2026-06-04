// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class CopyAction : FieldActionBase
    {
        public override int DefaultSortingIndex => 1000;
        public override string BadgeGlyph => Icons.Copy;
        public override string ActionType => string.Format(Consts.CopyActionPattern, FieldName);
        public override string DisplayName => $"Copy '{FieldName}'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.CopyField(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid, FieldName);
        }
    }
}
