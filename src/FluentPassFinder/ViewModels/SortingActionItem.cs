// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.ViewModels
{
    /// <summary>A single row in the action-order editor: a raw action type and its sort index.</summary>
    internal partial class SortingActionItem : ObservableObject
    {
        [ObservableProperty] private string actionType;
        [ObservableProperty] private decimal? index;
    }
}
