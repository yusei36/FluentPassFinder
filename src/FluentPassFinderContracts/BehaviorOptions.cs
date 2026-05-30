// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Behavioral options for how the search window reacts to user interaction.</summary>
    public class BehaviorOptions
    {
        /// <summary>Keep the previous search text when the window is reopened.</summary>
        public bool PreserveLastSearch { get; set; }

        /// <summary>How long a preserved search is kept before it is cleared.</summary>
        public int PreserveLastSearchTimeoutMilliseconds { get; set; } = 30_000;

        /// <summary>When true, Esc always closes the window instead of navigating back.</summary>
        public bool EscAlwaysClosesWindow { get; set; }
    }
}
