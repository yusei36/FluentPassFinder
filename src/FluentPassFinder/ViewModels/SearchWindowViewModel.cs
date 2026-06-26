// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Threading;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.Collections.ObjectModel;

namespace FluentPassFinder.ViewModels
{
    internal partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;
        private readonly IEntrySearchService entrySearchService;
        private readonly IEntryActionService entryActionService;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;
        private readonly SettingsViewModel settingsViewModel;
        private readonly CreateEntryViewModel createEntryViewModel;
        private CancellationTokenSource _searchCts;

        [ObservableProperty]
        private string applicationTitle = "FluentPassFinder";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EntryViewModel> entries = new();

        [ObservableProperty]
        private EntryViewModel selectedEntry;

        [ObservableProperty]
        private bool isEntryListUpdating;

        [ObservableProperty]
        private bool isContextMenuOpen = false;

        [ObservableProperty]
        private bool isSettingsOpen = false;

        [ObservableProperty]
        private bool isCreateEntryOpen = false;

        [ObservableProperty]
        private bool isPinned = false;

        public bool IsSearchBarVisible => !IsContextMenuOpen && !IsSettingsOpen && !IsCreateEntryOpen;
        public bool IsResultsVisible => !IsSettingsOpen && !IsCreateEntryOpen;

        // While an overlay (create/settings) is open the search-list key bindings (Enter family,
        // Up/Down) must go inert so those keys reach the focused control instead. A KeyBinding
        // whose command can't execute does not mark the key handled, so it falls through.
        public bool CanNavigateResults => !IsSettingsOpen && !IsCreateEntryOpen;

        [ObservableProperty]
        private ObservableCollection<IAction> contextActions;

        [ObservableProperty]
        private IAction selectedContextAction;

        public bool IsAnyDatabaseOpen => pluginProxy.IsAnyDatabaseOpen;
        internal Settings Settings => pluginProxy.Settings;

        public SearchWindowViewModel(IPluginProxy pluginProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService, ISearchWindowInteractionService searchWindowInteractionService, SettingsViewModel settingsViewModel, CreateEntryViewModel createEntryViewModel)
        {
            this.pluginProxy = pluginProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
            this.searchWindowInteractionService = searchWindowInteractionService;
            this.settingsViewModel = settingsViewModel;
            this.createEntryViewModel = createEntryViewModel;
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void EnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;

            if (IsContextMenuOpen)
            {
                if (SelectedContextAction == null) return;
                entryActionService.RunAction(entry.SearchResult, SelectedContextAction);
            }
            else
            {
                SelectedEntry = entry;
                entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.Actions.Main);
            }
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void ShiftEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.Actions.Shift);
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void ControlEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.Actions.Control);
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void AltEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.Actions.Alt);
        }

        [RelayCommand]
        private void RunAction(string actionType)
        {
            if (SelectedEntry == null) return;
            entryActionService.RunAction(SelectedEntry.SearchResult, actionType);
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void NavigateListDown()
        {
            if (IsContextMenuOpen)
                NavigateCollectionDown(ContextActions, SelectedContextAction, x => SelectedContextAction = x);
            else
                NavigateCollectionDown(Entries, SelectedEntry, x => SelectedEntry = x);
        }

        [RelayCommand(CanExecute = nameof(CanNavigateResults))]
        private void NavigateListUp()
        {
            if (IsContextMenuOpen)
                NavigateCollectionUp(ContextActions, SelectedContextAction, x => SelectedContextAction = x);
            else
                NavigateCollectionUp(Entries, SelectedEntry, x => SelectedEntry = x);
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void TogglePin()
        {
            IsPinned = !IsPinned;
        }

        [RelayCommand]
        private void ToggleSettings()
        {
            IsSettingsOpen = !IsSettingsOpen;
            if (IsSettingsOpen)
                settingsViewModel.ReloadGroups();
        }

        [RelayCommand]
        public void OpenCreateEntry(string preselectTemplateUuid = null)
        {
            if (!IsAnyDatabaseOpen) return;

            IsContextMenuOpen = false;
            IsSettingsOpen = false;
            createEntryViewModel.Open(preselectTemplateUuid, preselectTemplateUuid == null ? SearchText : null);
            IsCreateEntryOpen = true;
        }

        public void CloseCreateEntry()
        {
            IsCreateEntryOpen = false;
            searchWindowInteractionService.FocusSearchBox();
        }

        public void CloseWindowAfterCreate()
        {
            IsCreateEntryOpen = false;
            searchWindowInteractionService.Close();
        }

        [RelayCommand]
        private void SaveCreateEntry()
        {
            if (IsCreateEntryOpen)
                createEntryViewModel.CreateCommand.Execute(null);
        }

        [RelayCommand]
        private void NavigateBack()
        {
            if (IsSettingsOpen)
            {
                settingsViewModel.Discard();
                IsSettingsOpen = false;
                searchWindowInteractionService.FocusSearchBox();
            }
            else if (IsCreateEntryOpen)
            {
                CloseCreateEntry();
            }
            else if (IsContextMenuOpen && !Settings.Behavior.EscAlwaysClosesWindow)
            {
                IsContextMenuOpen = false;
                searchWindowInteractionService.FocusSearchBox();
            }
            else
            {
                searchWindowInteractionService.Close();
            }
        }

        [RelayCommand(CanExecute = nameof(IsContextMenuOpen))]
        private void CloseContextMenu()
        {
            IsContextMenuOpen = false;
            searchWindowInteractionService.FocusSearchBox();
        }

        [RelayCommand]
        private void SaveSettingsAndClose()
        {
            settingsViewModel.SaveCommand.Execute(null);
            IsSettingsOpen = false;
            searchWindowInteractionService.FocusSearchBox();
        }

        partial void OnIsContextMenuOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSearchBarVisible));
            CloseContextMenuCommand.NotifyCanExecuteChanged();
            if (value && SelectedEntry != null)
            {
                ContextActions = new ObservableCollection<IAction>(entryActionService.GetActionsForEntry(SelectedEntry.SearchResult));
                SelectedContextAction = ContextActions.Count > 0 ? ContextActions[0] : null;
            }
            else if (!value)
            {
                ContextActions = null;
                SelectedContextAction = null;
            }
        }
        partial void OnIsSettingsOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSearchBarVisible));
            OnPropertyChanged(nameof(IsResultsVisible));
            NotifyNavigateResultsCanExecuteChanged();
        }

        partial void OnIsCreateEntryOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSearchBarVisible));
            OnPropertyChanged(nameof(IsResultsVisible));
            NotifyNavigateResultsCanExecuteChanged();
        }

        private void NotifyNavigateResultsCanExecuteChanged()
        {
            OnPropertyChanged(nameof(CanNavigateResults));
            EnterActionCommand.NotifyCanExecuteChanged();
            ShiftEnterActionCommand.NotifyCanExecuteChanged();
            ControlEnterActionCommand.NotifyCanExecuteChanged();
            AltEnterActionCommand.NotifyCanExecuteChanged();
            NavigateListDownCommand.NotifyCanExecuteChanged();
            NavigateListUpCommand.NotifyCanExecuteChanged();
        }

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            _ = SearchAsync(value, _searchCts.Token);
        }

        private async Task SearchAsync(string query, CancellationToken ct)
        {
            if (IsContextMenuOpen) return;

            if (string.IsNullOrEmpty(query))
            {
                SelectedEntry = null;
                ClearEntries();
                return;
            }

            try { await Task.Delay(150, ct); }
            catch (OperationCanceledException) { return; }

            List<EntryViewModel> results;
            try
            {
                results = await Task.Run(() =>
                {
                    if (!pluginProxy.IsAnyDatabaseOpen) return null;
                    return entrySearchService.SearchEntries(query).Select(r => new EntryViewModel(r)).ToList();
                }, ct);
            }
            catch (OperationCanceledException) { return; }

            SelectedEntry = null;
            ClearEntries();

            if (results == null) return;

            IsEntryListUpdating = true;
            Entries = new ObservableCollection<EntryViewModel>(results);
            SelectedEntry = Entries.Count > 0 ? Entries[0] : null;

            Dispatcher.UIThread.Post(() => IsEntryListUpdating = false, DispatcherPriority.Background);
        }

        internal void ClearEntries()
        {
            foreach (var entry in Entries)
                entry.Dispose();
            Entries.Clear();
        }

        private static void NavigateCollectionDown<T>(ObservableCollection<T> collection, T selectedItem, System.Action<T> update)
        {
            if (selectedItem == null) return;
            var idx = collection.IndexOf(selectedItem);
            if (idx + 1 < collection.Count)
                update(collection[idx + 1]);
        }

        private static void NavigateCollectionUp<T>(ObservableCollection<T> collection, T selectedItem, System.Action<T> update)
        {
            if (selectedItem == null) return;
            var idx = collection.IndexOf(selectedItem);
            if (idx - 1 >= 0)
                update(collection[idx - 1]);
        }
    }
}
