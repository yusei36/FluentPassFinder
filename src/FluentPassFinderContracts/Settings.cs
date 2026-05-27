// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    public class Settings
    {
        public SearchOptions SearchOptions { get; set; } = new SearchOptions();
        public string PluginTotpPlaceholder { get; set; }
        public string PluginTotpFieldConfig { get; set; }

        public string MainAction { get; set; }
        public string ShiftAction { get; set; }
        public string ControlAction { get; set; }
        public string AltAction { get; set; }

        public Dictionary<string, int> ActionSorting { get; set; } = new Dictionary<string, int>();
        public bool ShowActionsForCustomFields { get; set; }
        public List<string> ExcludeActionsForFields { get; set; } = new List<string>();

        public string GlobalHotkeyCurrentScreen { get; set; }
        public string GlobalHotkeyPrimaryScreen { get; set; }
        public string Theme { get; set; }

        public bool PreserveLastSearch { get; set; }
        public int PreserveLastSearchTimeoutMilliseconds { get; set; }
        public bool EscAlwaysClosesWindow { get; set; }

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public WindowAnchor WindowAnchor { get; set; }

        public static Settings DefaultSettings = new Settings()
        {
            SearchOptions = new SearchOptions()
            {
                IncludeTitleField = true,
                IncludeNotesField = true,
                IncludeUrlField = true,
                IncludeCustomFields = true,
                IncludeTags = true,
                ExcludeExpiredEntries = true,
                ExcludeGroupsBySearchSetting = true,
                ResolveFieldReferences = true,
            },
            PluginTotpPlaceholder = Consts.PluginTotpPlaceholder,
            PluginTotpFieldConfig = Consts.PluginTotpFieldConfigKey,
            GlobalHotkeyCurrentScreen = "Ctrl+Alt+S",
            GlobalHotkeyPrimaryScreen = "Ctrl+Alt+F",
            MainAction = Consts.OpenContextMenu,
            ShiftAction = string.Format(Consts.CopyActionPattern, Consts.UserNameField),
            ControlAction = string.Format(Consts.CopyActionPattern, Consts.PasswordField),
            AltAction = string.Format(Consts.CopyActionPattern, Consts.Totp),
            ActionSorting = new Dictionary<string, int>
            {
                { string.Format(Consts.AutoTypeActionPattern, Consts.UserNameField), 1 },
                { string.Format(Consts.AutoTypeActionPattern, Consts.PasswordField), 2 },
                { string.Format(Consts.AutoTypeActionPattern, Consts.Totp), 3 },

                { string.Format(Consts.CopyActionPattern, Consts.UserNameField), 101 },
                { string.Format(Consts.CopyActionPattern, Consts.PasswordField), 102 },
                { string.Format(Consts.CopyActionPattern, Consts.Totp), 103 },
            },
            ShowActionsForCustomFields = true,
            ExcludeActionsForFields = new List<string> { Consts.TemplateUuidField },
            Theme = "Dark",
            PreserveLastSearch = false,
            PreserveLastSearchTimeoutMilliseconds = 30_000,
            EscAlwaysClosesWindow = false,
            WindowWidth = 450,
            WindowHeight = 400,
            WindowAnchor = WindowAnchor.CenterCenter,
        };
    }
}
