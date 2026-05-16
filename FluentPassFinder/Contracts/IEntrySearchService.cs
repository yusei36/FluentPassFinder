// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;

namespace FluentPassFinder.Contracts
{
    internal interface IEntrySearchService
    {
        IEnumerable<EntrySearchResult> SearchEntries(string query);
    }
}
