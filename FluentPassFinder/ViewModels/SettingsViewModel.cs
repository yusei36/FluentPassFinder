using Avalonia;
using Avalonia.Styling;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.ViewModels
{
    internal partial class SettingsViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;

        [ObservableProperty] private string theme;
        [ObservableProperty] private string globalHotkeyCurrentScreen;
        [ObservableProperty] private string globalHotkeyPrimaryScreen;
        [ObservableProperty] private bool showActionsForCustomFields;

        [ObservableProperty] private bool includeTitleField;
        [ObservableProperty] private bool includeUserNameField;
        [ObservableProperty] private bool includePasswordField;
        [ObservableProperty] private bool includeUrlField;
        [ObservableProperty] private bool includeNotesField;
        [ObservableProperty] private bool includeTags;
        [ObservableProperty] private bool includeCustomFields;
        [ObservableProperty] private bool includeProtectedCustomFields;

        [ObservableProperty] private bool excludeExpiredEntries;
        [ObservableProperty] private bool excludeGroupsBySearchSetting;
        [ObservableProperty] private bool resolveFieldReferences;

        [ObservableProperty] private string mainAction;
        [ObservableProperty] private string shiftAction;
        [ObservableProperty] private string controlAction;
        [ObservableProperty] private string altAction;

        [ObservableProperty] private string pluginTotpPlaceholder;
        [ObservableProperty] private string pluginTotpFieldConfig;

        public static System.Collections.Generic.IReadOnlyList<string> AvailableThemes { get; } =
            new[] { "Dark", "Light" };

        public static System.Collections.Generic.IReadOnlyList<string> AvailableActions { get; } =
            new[]
            {
                Consts.OpenContextMenu,
                Consts.SelectEntry,
                Consts.OpenUrl,
                string.Format(Consts.CopyActionPattern, Consts.UserNameField),
                string.Format(Consts.CopyActionPattern, Consts.PasswordField),
                string.Format(Consts.CopyActionPattern, Consts.TitleField),
                string.Format(Consts.CopyActionPattern, Consts.NotesField),
                string.Format(Consts.CopyActionPattern, Consts.UrlField),
                string.Format(Consts.CopyActionPattern, Consts.Totp),
                string.Format(Consts.AutoTypeActionPattern, Consts.UserNameField),
                string.Format(Consts.AutoTypeActionPattern, Consts.PasswordField),
                string.Format(Consts.AutoTypeActionPattern, Consts.Totp),
            };

        public SettingsViewModel(IPluginProxy pluginProxy)
        {
            this.pluginProxy = pluginProxy;
            LoadFromSettings(pluginProxy.Settings);
        }

        [RelayCommand]
        private void Save()
        {
            var original = pluginProxy.Settings;
            var newSettings = new Settings
            {
                Theme = Theme,
                GlobalHotkeyCurrentScreen = GlobalHotkeyCurrentScreen,
                GlobalHotkeyPrimaryScreen = GlobalHotkeyPrimaryScreen,
                SearchOptions = new SearchOptions
                {
                    IncludeTitleField = IncludeTitleField,
                    IncludeUserNameField = IncludeUserNameField,
                    IncludePasswordField = IncludePasswordField,
                    IncludeUrlField = IncludeUrlField,
                    IncludeNotesField = IncludeNotesField,
                    IncludeTags = IncludeTags,
                    IncludeCustomFields = IncludeCustomFields,
                    IncludeProtectedCustomFields = IncludeProtectedCustomFields,
                    ExcludeExpiredEntries = ExcludeExpiredEntries,
                    ExcludeGroupsBySearchSetting = ExcludeGroupsBySearchSetting,
                    ResolveFieldReferences = ResolveFieldReferences,
                },
                MainAction = MainAction,
                ShiftAction = ShiftAction,
                ControlAction = ControlAction,
                AltAction = AltAction,
                ShowActionsForCustomFields = ShowActionsForCustomFields,
                PluginTotpPlaceholder = PluginTotpPlaceholder,
                PluginTotpFieldConfig = PluginTotpFieldConfig,
                // Preserve fields without UI editors
                ActionSorting = original.ActionSorting,
                ExcludeActionsForFields = original.ExcludeActionsForFields,
            };

            pluginProxy.SaveSettings(newSettings);
            App.ApplySettings(newSettings);
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            LoadFromSettings(Settings.DefaultSettings);
        }

        private void LoadFromSettings(Settings s)
        {
            Theme = s.Theme;
            GlobalHotkeyCurrentScreen = s.GlobalHotkeyCurrentScreen;
            GlobalHotkeyPrimaryScreen = s.GlobalHotkeyPrimaryScreen;

            IncludeTitleField = s.SearchOptions.IncludeTitleField;
            IncludeUserNameField = s.SearchOptions.IncludeUserNameField;
            IncludePasswordField = s.SearchOptions.IncludePasswordField;
            IncludeUrlField = s.SearchOptions.IncludeUrlField;
            IncludeNotesField = s.SearchOptions.IncludeNotesField;
            IncludeTags = s.SearchOptions.IncludeTags;
            IncludeCustomFields = s.SearchOptions.IncludeCustomFields;
            IncludeProtectedCustomFields = s.SearchOptions.IncludeProtectedCustomFields;
            ExcludeExpiredEntries = s.SearchOptions.ExcludeExpiredEntries;
            ExcludeGroupsBySearchSetting = s.SearchOptions.ExcludeGroupsBySearchSetting;
            ResolveFieldReferences = s.SearchOptions.ResolveFieldReferences;

            MainAction = s.MainAction;
            ShiftAction = s.ShiftAction;
            ControlAction = s.ControlAction;
            AltAction = s.AltAction;
            ShowActionsForCustomFields = s.ShowActionsForCustomFields;

            PluginTotpPlaceholder = s.PluginTotpPlaceholder;
            PluginTotpFieldConfig = s.PluginTotpFieldConfig;
        }
    }
}
