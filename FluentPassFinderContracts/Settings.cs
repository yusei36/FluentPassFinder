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
            PluginTotpPlaceholder = "{TOTP}",
            PluginTotpFieldConfig = "totpsettings_stringname",
            GlobalHotkeyCurrentScreen = "Ctrl+Alt+S",
            GlobalHotkeyPrimaryScreen = "Ctrl+Alt+F",
            MainAction = ActionTypeConsts.OpenContextMenu,
            ShiftAction = string.Format(ActionTypeConsts.CopyActionPattern, "UserName"),
            ControlAction = string.Format(ActionTypeConsts.CopyActionPattern, "Password"),
            AltAction = string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp),
            ActionSorting = new Dictionary<string, int>
            {
                { string.Format(ActionTypeConsts.AutoTypeActionPattern, "UserName"), 1 },
                { string.Format(ActionTypeConsts.AutoTypeActionPattern, "Password"), 2 },
                { string.Format(ActionTypeConsts.AutoTypeActionPattern, ActionTypeConsts.Totp), 3 },

                { string.Format(ActionTypeConsts.CopyActionPattern, "UserName"), 101 },
                { string.Format(ActionTypeConsts.CopyActionPattern, "Password"), 102 },
                { string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp), 103 },
            },
            ShowActionsForCustomFields = true,
            ExcludeActionsForFields = new List<string> { "_etm_template_uuid" },
            Theme = "Dark"
        };
    }
}
