// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Action bindings and visibility rules for entry/field actions.</summary>
    public class ActionOptions
    {
        /// <summary>Action run on Enter.</summary>
        public string Main { get; set; } = Consts.OpenContextMenu;

        /// <summary>Action run on Shift+Enter.</summary>
        public string Shift { get; set; } = string.Format(Consts.CopyActionPattern, Consts.UserNameField);

        /// <summary>Action run on Ctrl+Enter.</summary>
        public string Control { get; set; } = string.Format(Consts.CopyActionPattern, Consts.PasswordField);

        /// <summary>Action run on Alt+Enter.</summary>
        public string Alt { get; set; } = string.Format(Consts.CopyActionPattern, Consts.Totp);

        /// <summary>Whether actions are offered for non-standard (custom) entry fields.</summary>
        public bool ShowForCustomFields { get; set; } = true;

        /// <summary>Field names for which no actions are shown.</summary>
        public List<string> ExcludeFields { get; set; } = new List<string> { Consts.TemplateUuidField };

        /// <summary>Per-action sort order, keyed by action type. Lower sorts first.</summary>
        public Dictionary<string, int> Sorting { get; set; } = CreateDefaultSorting();

        private static Dictionary<string, int> CreateDefaultSorting() => new Dictionary<string, int>
        {
            { string.Format(Consts.AutoTypeActionPattern, Consts.UserNameField), 1 },
            { string.Format(Consts.AutoTypeActionPattern, Consts.PasswordField), 2 },
            { string.Format(Consts.AutoTypeActionPattern, Consts.Totp), 3 },

            { string.Format(Consts.CopyActionPattern, Consts.UserNameField), 101 },
            { string.Format(Consts.CopyActionPattern, Consts.PasswordField), 102 },
            { string.Format(Consts.CopyActionPattern, Consts.Totp), 103 },
        };
    }
}
