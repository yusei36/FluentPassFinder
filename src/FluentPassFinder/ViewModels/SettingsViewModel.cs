// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
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

        [ObservableProperty] private bool preserveLastSearch;
        [ObservableProperty] private decimal? preserveLastSearchTimeoutSeconds;
        [ObservableProperty] private bool escAlwaysClosesWindow;

        [ObservableProperty] private decimal? windowWidth;
        [ObservableProperty] private decimal? windowHeight;
        [ObservableProperty] private WindowAnchor windowAnchor;
        [ObservableProperty] private decimal? windowOffsetX;
        [ObservableProperty] private decimal? windowOffsetY;

        public static WindowAnchor[] AvailableWindowAnchors { get; } = new[]
        {
            WindowAnchor.CenterCenter, WindowAnchor.CenterTop, WindowAnchor.CenterBottom,
            WindowAnchor.LeftCenter,   WindowAnchor.LeftTop,   WindowAnchor.LeftBottom,
            WindowAnchor.RightCenter,  WindowAnchor.RightTop,  WindowAnchor.RightBottom,
        };

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
            var defaults = Settings.CreateDefault();
            var newSettings = new Settings
            {
                Version = original.Version,
                Theme = Enum.TryParse<AppTheme>(Theme, out var theme) ? theme : AppTheme.Dark,
                Hotkeys = new HotkeyOptions
                {
                    CurrentScreen = GlobalHotkeyCurrentScreen,
                    PrimaryScreen = GlobalHotkeyPrimaryScreen,
                },
                Search = new SearchOptions
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
                Actions = new ActionOptions
                {
                    Main = MainAction,
                    Shift = ShiftAction,
                    Control = ControlAction,
                    Alt = AltAction,
                    ShowForCustomFields = ShowActionsForCustomFields,
                    // Preserve fields without UI editors
                    Sorting = original.Actions.Sorting,
                    ExcludeForFields = original.Actions.ExcludeForFields,
                },
                Totp = new TotpOptions
                {
                    Placeholder = PluginTotpPlaceholder,
                    FieldConfigKey = PluginTotpFieldConfig,
                },
                Behavior = new BehaviorOptions
                {
                    PreserveLastSearch = PreserveLastSearch,
                    PreserveLastSearchTimeoutMilliseconds = (int)(PreserveLastSearchTimeoutSeconds ?? 30) * 1000,
                    EscAlwaysClosesWindow = EscAlwaysClosesWindow,
                },
                Window = new WindowOptions
                {
                    Width = (int)(WindowWidth ?? defaults.Window.Width),
                    Height = (int)(WindowHeight ?? defaults.Window.Height),
                    Anchor = WindowAnchor,
                    OffsetX = (int)(WindowOffsetX ?? 0),
                    OffsetY = (int)(WindowOffsetY ?? 0),
                },
            };

            pluginProxy.SaveSettings(newSettings);
            App.ApplySettings(newSettings);
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            LoadFromSettings(Settings.CreateDefault());
        }

        private void LoadFromSettings(Settings s)
        {
            Theme = s.Theme.ToString();
            GlobalHotkeyCurrentScreen = s.Hotkeys.CurrentScreen;
            GlobalHotkeyPrimaryScreen = s.Hotkeys.PrimaryScreen;

            IncludeTitleField = s.Search.IncludeTitleField;
            IncludeUserNameField = s.Search.IncludeUserNameField;
            IncludePasswordField = s.Search.IncludePasswordField;
            IncludeUrlField = s.Search.IncludeUrlField;
            IncludeNotesField = s.Search.IncludeNotesField;
            IncludeTags = s.Search.IncludeTags;
            IncludeCustomFields = s.Search.IncludeCustomFields;
            IncludeProtectedCustomFields = s.Search.IncludeProtectedCustomFields;
            ExcludeExpiredEntries = s.Search.ExcludeExpiredEntries;
            ExcludeGroupsBySearchSetting = s.Search.ExcludeGroupsBySearchSetting;
            ResolveFieldReferences = s.Search.ResolveFieldReferences;

            MainAction = s.Actions.Main;
            ShiftAction = s.Actions.Shift;
            ControlAction = s.Actions.Control;
            AltAction = s.Actions.Alt;
            ShowActionsForCustomFields = s.Actions.ShowForCustomFields;

            PluginTotpPlaceholder = s.Totp.Placeholder;
            PluginTotpFieldConfig = s.Totp.FieldConfigKey;

            PreserveLastSearch = s.Behavior.PreserveLastSearch;
            PreserveLastSearchTimeoutSeconds = s.Behavior.PreserveLastSearchTimeoutMilliseconds / 1000;
            EscAlwaysClosesWindow = s.Behavior.EscAlwaysClosesWindow;

            var defaults = Settings.CreateDefault();
            WindowWidth = s.Window.Width > 0 ? s.Window.Width : defaults.Window.Width;
            WindowHeight = s.Window.Height > 0 ? s.Window.Height : defaults.Window.Height;
            WindowAnchor = s.Window.Anchor;
            WindowOffsetX = s.Window.OffsetX;
            WindowOffsetY = s.Window.OffsetY;
        }
    }
}
