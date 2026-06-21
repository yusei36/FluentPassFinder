// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Options controlling how new entries are created from the search window.</summary>
    public class EntryCreationOptions
    {
        /// <summary>
        /// Hex UUID of the group new entries are saved into. Defaults to a dedicated group that
        /// is auto-created as "New entries" (<see cref="Consts.DefaultNewEntryGroupName"/>) on
        /// first use if it does not already exist.
        /// </summary>
        public string NewEntryGroupUuid { get; set; } = Consts.DefaultNewEntryGroupUuid;
    }
}
