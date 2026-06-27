// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Global hotkeys that open the search window.</summary>
    public class HotkeyOptions
    {
        /// <summary>Opens the search window on the screen under the cursor.</summary>
        public string CurrentScreen { get; set; } = "Ctrl+Alt+S";

        /// <summary>Opens the search window on the primary screen.</summary>
        public string PrimaryScreen { get; set; } = "Ctrl+Alt+F";

        /// <summary>Opens the search window (current screen) with the create-entry overlay.</summary>
        public string NewEntry { get; set; } = "Ctrl+Alt+N";
    }
}
