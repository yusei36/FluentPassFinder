using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentPassFinder.ViewModels
{
    internal partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;
        private readonly IEntrySearchService entrySearchService;
        private readonly IEntryActionService entryActionService;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;
        private readonly SettingsViewModel settingsViewModel;
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
        private bool isContextMenuOpen = false;

        [ObservableProperty]
        private bool isSettingsOpen = false;

        public bool IsSearchBarVisible => !IsContextMenuOpen && !IsSettingsOpen;

        [ObservableProperty]
        private ObservableCollection<IAction> contextActions;

        [ObservableProperty]
        private IAction selectedContextAction;

        public bool IsAnyDatabaseOpen => pluginProxy.IsAnyDatabaseOpen;

        public SearchWindowViewModel(IPluginProxy pluginProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService, ISearchWindowInteractionService searchWindowInteractionService, SettingsViewModel settingsViewModel)
        {
            this.pluginProxy = pluginProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
            this.searchWindowInteractionService = searchWindowInteractionService;
            this.settingsViewModel = settingsViewModel;
        }

        [RelayCommand]
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
                entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.MainAction);
            }
        }

        [RelayCommand]
        private void ShiftEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.ShiftAction);
        }

        [RelayCommand]
        private void ControlEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.ControlAction);
        }

        [RelayCommand]
        private void AltEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null) return;
            entryActionService.RunAction(entry.SearchResult, pluginProxy.Settings.AltAction);
        }

        [RelayCommand]
        private void RunAction(string actionType)
        {
            if (SelectedEntry == null) return;
            entryActionService.RunAction(SelectedEntry.SearchResult, actionType);
        }

        [RelayCommand]
        private void NavigateListDown()
        {
            if (IsContextMenuOpen)
                NavigateCollectionDown(ContextActions, SelectedContextAction, x => SelectedContextAction = x);
            else
                NavigateCollectionDown(Entries, SelectedEntry, x => SelectedEntry = x);
        }

        [RelayCommand]
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
        private void ToggleSettings()
        {
            IsSettingsOpen = !IsSettingsOpen;
        }

        [RelayCommand]
        private void NavigateBack()
        {
            if (IsSettingsOpen)
            {
                IsSettingsOpen = false;
                searchWindowInteractionService.FocusSearchBox();
            }
            // TODO: think about a option to navigate back from context menu or make it the default behavior
            //else if (IsContextMenuOpen)
            //{
            //    IsContextMenuOpen = false;
            //    searchWindowInteractionService.FocusSearchBox();
            //}
            else
            {
                searchWindowInteractionService.Close();
            }
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
            if (value && SelectedEntry != null)
            {
                ContextActions = new ObservableCollection<IAction>(entryActionService.GetActionsForEntry(SelectedEntry.SearchResult, false));
                SelectedContextAction = ContextActions.Count > 0 ? ContextActions[0] : null;
            }
            else if (!value)
            {
                ContextActions = null;
                SelectedContextAction = null;
            }
        }
        partial void OnIsSettingsOpenChanged(bool value) => OnPropertyChanged(nameof(IsSearchBarVisible));

        partial void OnSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            _ = SearchAsync(value, _searchCts.Token);
        }

        private async Task SearchAsync(string query, CancellationToken ct)
        {
            if (IsContextMenuOpen) return;

            SelectedEntry = null;
            ClearEntries();

            try { await Task.Delay(250, ct); }
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

            if (results == null) return;

            Entries = new ObservableCollection<EntryViewModel>(results);
            SelectedEntry = Entries.Count > 0 ? Entries[0] : null;
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
