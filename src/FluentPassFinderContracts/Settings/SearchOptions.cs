// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Controls which entry fields are searched and how results are filtered.</summary>
    public class SearchOptions
    {
        public bool IncludeTitleField { get; set; } = true;
        public bool IncludeUserNameField { get; set; }
        public bool IncludePasswordField { get; set; }
        public bool IncludeUrlField { get; set; } = true;
        public bool IncludeNotesField { get; set; } = true;
        public bool IncludeTags { get; set; } = true;
        public bool IncludeCustomFields { get; set; } = true;
        public bool IncludeProtectedCustomFields { get; set; }

        public bool ExcludeExpiredEntries { get; set; } = true;
        public bool ExcludeGroupsBySearchSetting { get; set; } = true;

        public bool ResolveFieldReferences { get; set; } = true;
    }
}
