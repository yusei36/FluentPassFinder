// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class AutoTypeAction : FieldActionBase
    {
        public override int DefaultSortingIndex => 2000;
        public override string BadgePath => Icons.Keyboard;
        public override string ActionType => string.Format(Consts.AutoTypeActionPattern, FieldName);
        public override string DisplayName => $"Auto type '{FieldName}'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            var fieldName = FieldName;
            searchWindowInteractionService.CloseThen(() =>
                pluginProxy.AutoTypeField(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid, fieldName));
        }
    }
}
