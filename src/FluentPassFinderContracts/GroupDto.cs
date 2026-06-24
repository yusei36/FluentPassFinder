// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>A group in the active database, offered as a target for new entries.</summary>
    public class GroupDto
    {
        /// <summary>Hex-encoded group UUID.</summary>
        public string Uuid { get; set; }

        public string Name { get; set; }

        /// <summary>Full path from the root group, for display (e.g. "Database / Folder / Sub").</summary>
        public string Path { get; set; }

        /// <summary>Pre-rendered 24x24 icon as PNG bytes.</summary>
        public byte[] Icon { get; set; }
    }
}
