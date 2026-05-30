// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Size and placement of the search window.</summary>
    public class WindowOptions
    {
        public int Width { get; set; } = 450;

        /// <summary>Maximum height of the results area (excludes the search bar header).</summary>
        public int Height { get; set; } = 400;

        public WindowAnchor Anchor { get; set; } = WindowAnchor.CenterCenter;

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
    }
}
