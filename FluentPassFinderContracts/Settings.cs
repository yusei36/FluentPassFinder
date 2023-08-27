using KeePassLib;
using System.Collections.Generic;

namespace FluentPassFinder.Contracts.Public
{
    public class Settings
    {
        public SearchOptions SearchOptions { get; set; } = new();
        public string PluginTotpPlaceholder { get; set; }
        public string PluginTotpFieldConfig { get; set; }

        public string MainAction { get; set; }
        public string ShiftAction { get; set; }
        public string ControlAction { get; set; }
        public string AltAction { get; set; }

        public Dictionary<string, int> ActionSorting { get; set; } = new();
        public bool ShowActionsForCustomFields { get; set; }
        public List<string> ExcludeActionsForFields { get; set; } = new();


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
            ShiftAction = string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.UserNameField),
            ControlAction = string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.PasswordField),
            AltAction = string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp),
            ActionSorting = new Dictionary<string, int>
                {
                    { string.Format(ActionTypeConsts.AutoTypeActionPattern, PwDefs.UserNameField), 1 },
                    { string.Format(ActionTypeConsts.AutoTypeActionPattern, PwDefs.PasswordField), 2 },
                    { string.Format(ActionTypeConsts.AutoTypeActionPattern, ActionTypeConsts.Totp), 3 },

                    { string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.UserNameField), 101 },
                    { string.Format(ActionTypeConsts.CopyActionPattern, PwDefs.PasswordField), 102 },
                    { string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp), 103 },
                },
            ShowActionsForCustomFields = true,
            ExcludeActionsForFields = new List<string>()
                {
                    "_etm_template_uuid"
                },
            Theme = "Dark"
        };
    }
}
