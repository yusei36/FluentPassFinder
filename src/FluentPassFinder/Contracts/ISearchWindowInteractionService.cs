// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts
{
    internal interface ISearchWindowInteractionService
    {
        void Close();
        void CloseThen(System.Action action);
        void FocusSearchBox();
    }
}