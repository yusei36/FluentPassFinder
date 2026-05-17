// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;
using System.Windows.Input;

namespace FluentPassFinder.Contracts
{
    internal interface IAction : ICommand
    {
        string ActionType { get; }
        int SortingIndex { get; }
        string DisplayName { get; }
        string IconPath { get; }
        string BadgePath { get; }

        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService);
        void RunAction(EntrySearchResult searchResult);
    }
}
