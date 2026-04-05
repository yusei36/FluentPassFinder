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
        private ObservableCollection<IAction> contextActions;

        [ObservableProperty]
        private IAction selectedContextAction;

        public bool IsAnyDatabaseOpen => pluginProxy.IsAnyDatabaseOpen;

        public SearchWindowViewModel(IPluginProxy pluginProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService)
        {
            this.pluginProxy = pluginProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
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

        partial void OnSearchTextChanged(string value)
        {
            if (IsContextMenuOpen) return;

            SelectedEntry = null;
            Entries.Clear();

            if (!pluginProxy.IsAnyDatabaseOpen) return;

            foreach (var result in entrySearchService.SearchEntries(value))
                Entries.Add(new EntryViewModel(result));

            SelectedEntry = Entries.Count > 0 ? Entries[0] : null;
        }

        partial void OnSelectedEntryChanged(EntryViewModel oldValue, EntryViewModel newValue)
        {
            if (newValue == null)
            {
                ContextActions?.Clear();
                SelectedContextAction = null;
                return;
            }

            ContextActions = new ObservableCollection<IAction>(entryActionService.GetActionsForEntry(newValue.SearchResult, false));
            SelectedContextAction = ContextActions.Count > 0 ? ContextActions[0] : null;
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
