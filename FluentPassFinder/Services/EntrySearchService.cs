// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.Collections.Generic;
using System.Linq;

namespace FluentPassFinder.Services
{
    internal class EntrySearchService : IEntrySearchService
    {
        private readonly IPluginProxy pluginProxy;

        public EntrySearchService(IPluginProxy pluginProxy)
        {
            this.pluginProxy = pluginProxy;
        }

        public IEnumerable<EntrySearchResult> SearchEntries(string query)
        {
            return pluginProxy.SearchEntries(query)
                              .Select(e => new EntrySearchResult { Entry = e });
        }
    }
}
