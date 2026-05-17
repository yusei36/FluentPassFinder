// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    internal interface IFieldAction : IAction
    {
        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService, string fieldName);
    }
}
