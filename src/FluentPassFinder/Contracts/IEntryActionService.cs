// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    internal interface IEntryActionService
    {
        void RunAction(EntrySearchResult searchResult, string actionType);
        void RunAction(EntrySearchResult searchResult, IAction action);
        IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult, bool includeHiddenActions);
    }
}
