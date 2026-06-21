// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
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
        [ObservableProperty] private string newExcludeField;

        public ObservableCollection<string> ExcludeFields { get; } = new ObservableCollection<string>();

        [ObservableProperty] private string newSortingAction;
        public ObservableCollection<SortingActionItem> SortingActions { get; } = new ObservableCollection<SortingActionItem>();

        [ObservableProperty] private string totpPlaceholder;

        [ObservableProperty] private bool preserveLastSearch;
        [ObservableProperty] private decimal? preserveLastSearchTimeoutSeconds;
        [ObservableProperty] private bool escAlwaysClosesWindow;

        [ObservableProperty] private ObservableCollection<GroupDto> availableGroups = new();
        [ObservableProperty] private GroupDto selectedGroup;
        private string configuredGroupUuid;

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
            Enum.GetNames(typeof(AppTheme));

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

        // About
        public static string AppVersionFull { get; } =
            Assembly.GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "?";

        // Trim the git commit hash that follows "+" down to the first 8 chars for display.
        public static string AppVersion { get; } =
            Regex.Replace(AppVersionFull, @"\+([0-9a-f]{8})[0-9a-f]+", "+$1", RegexOptions.IgnoreCase);

        public const string Copyright = "© 2023 - 2026 Uwe Kögel";
        public const string ProjectUrl = "https://github.com/yusei36/FluentPassFinder";
        public const string IssuesUrl = "https://github.com/yusei36/FluentPassFinder/issues";
        public const string LicenseUrl = "https://github.com/yusei36/FluentPassFinder/blob/master/LICENSE";

        public SettingsViewModel(IPluginProxy pluginProxy)
        {
            this.pluginProxy = pluginProxy;
            LoadFromSettings(pluginProxy.Settings);
        }

        [RelayCommand]
        private void OpenUrl(string url) =>
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });

        [RelayCommand]
        private async System.Threading.Tasks.Task CopyToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
                await App.CopyToClipboardAsync(text);
        }

        [RelayCommand]
        private void Save()
        {
            var original = pluginProxy.Settings;
            var defaults = Settings.CreateDefault();
            var newSettings = new Settings
            {
                Version = original.Version,
                Theme = Enum.TryParse<AppTheme>(Theme, out var theme) ? theme : AppTheme.System,
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
                    ExcludeFields = ExcludeFields.Distinct().ToList(),
                    Sorting = SortingActions
                        .Where(a => !string.IsNullOrWhiteSpace(a.ActionType))
                        .ToDictionary(a => a.ActionType.Trim(), a => (int)(a.Index ?? 0)),
                },
                Otp = new OtpOptions
                {
                    TotpPlaceholder = TotpPlaceholder,
                },
                Behavior = new BehaviorOptions
                {
                    PreserveLastSearch = PreserveLastSearch,
                    PreserveLastSearchTimeoutMilliseconds = (int)(PreserveLastSearchTimeoutSeconds ?? 30) * 1000,
                    EscAlwaysClosesWindow = EscAlwaysClosesWindow,
                },
                EntryCreation = new EntryCreationOptions
                {
                    NewEntryGroupUuid = SelectedGroup?.Uuid ?? Consts.DefaultNewEntryGroupUuid,
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
        private void AddExcludeField()
        {
            var field = NewExcludeField?.Trim();
            if (string.IsNullOrEmpty(field)) return;

            if (!ExcludeFields.Any(f => f.Equals(field, StringComparison.OrdinalIgnoreCase)))
                ExcludeFields.Add(field);

            NewExcludeField = string.Empty;
        }

        [RelayCommand]
        private void RemoveExcludeField(string field)
        {
            if (field != null)
                ExcludeFields.Remove(field);
        }

        [RelayCommand]
        private void AddSortingAction()
        {
            var actionType = NewSortingAction?.Trim();
            if (string.IsNullOrEmpty(actionType)) return;

            if (!SortingActions.Any(a => a.ActionType.Equals(actionType, StringComparison.OrdinalIgnoreCase)))
            {
                var nextIndex = SortingActions.Count == 0
                    ? 1
                    : (int)SortingActions.Max(a => a.Index ?? 0) + 1;
                SortingActions.Add(new SortingActionItem { ActionType = actionType, Index = nextIndex });
            }

            NewSortingAction = string.Empty;
        }

        [RelayCommand]
        private void RemoveSortingAction(SortingActionItem item)
        {
            if (item != null)
                SortingActions.Remove(item);
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            LoadFromSettings(Settings.CreateDefault());
        }

        /// <summary>
        /// Refreshes the list of target groups from the active database. Keeps the current
        /// selection if still present; otherwise ensures the configured group is selectable
        /// (the default "New entries" group does not exist until the first entry is created).
        /// </summary>
        public void ReloadGroups()
        {
            var targetUuid = SelectedGroup?.Uuid ?? configuredGroupUuid;
            AvailableGroups = GroupChoices.Build(pluginProxy.GetGroups(), targetUuid);
            SelectedGroup = GroupChoices.Select(AvailableGroups, targetUuid);
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

            ExcludeFields.Clear();
            foreach (var field in s.Actions.ExcludeFields)
                ExcludeFields.Add(field);
            NewExcludeField = string.Empty;

            SortingActions.Clear();
            if (s.Actions.Sorting != null)
                foreach (var kv in s.Actions.Sorting.OrderBy(kv => kv.Value))
                    SortingActions.Add(new SortingActionItem { ActionType = kv.Key, Index = kv.Value });
            NewSortingAction = string.Empty;

            TotpPlaceholder = s.Otp.TotpPlaceholder;

            PreserveLastSearch = s.Behavior.PreserveLastSearch;
            PreserveLastSearchTimeoutSeconds = s.Behavior.PreserveLastSearchTimeoutMilliseconds / 1000;
            EscAlwaysClosesWindow = s.Behavior.EscAlwaysClosesWindow;

            configuredGroupUuid = s.EntryCreation?.NewEntryGroupUuid ?? Consts.DefaultNewEntryGroupUuid;
            SelectedGroup = null; // force selection back to the configured group on (re)load / reset
            ReloadGroups();

            var defaults = Settings.CreateDefault();
            WindowWidth = s.Window.Width > 0 ? s.Window.Width : defaults.Window.Width;
            WindowHeight = s.Window.Height > 0 ? s.Window.Height : defaults.Window.Height;
            WindowAnchor = s.Window.Anchor;
            WindowOffsetX = s.Window.OffsetX;
            WindowOffsetY = s.Window.OffsetY;
        }
    }
}
