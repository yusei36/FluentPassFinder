using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using System.Collections.ObjectModel;

namespace FluentPassFinder.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;
        private readonly IEntrySearchService entrySearchService;
        private readonly IEntryActionService entryActionService;

        [ObservableProperty]
        private string applicationTitle = "FluentPassFinder";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EntryViewModel> entries = new ObservableCollection<EntryViewModel>();

        [ObservableProperty]
        private EntryViewModel selectedEntry;

        [ObservableProperty]
        private bool isContextMenuOpen = false;

        [ObservableProperty]
        private ObservableCollection<IAction> contextActions;

        [ObservableProperty]
        private IAction selectedContextAction;
        public Boolean IsAnyDatabaseOpen => pluginProxy.Databases.Any();

        public SearchWindowViewModel(IPluginProxy pluginProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService)
        {
            this.pluginProxy = pluginProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
            contextActions = new ObservableCollection<IAction>(entryActionService.Actions.Where(a => a.ActionType != ActionType.OpenContextMenu));
            selectedContextAction = contextActions.First();
        }

        [RelayCommand]
        private void EnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null)
            {
                return;
            }

            if (IsContextMenuOpen)
            {
                if (SelectedContextAction == null)
                {
                    return;
                }

                entryActionService.RunAction(entry.SearchResult, SelectedContextAction);
            }
            else 
            {
                SelectedEntry = entry;
                entryActionService.RunAction(entry.SearchResult, ActionType.OpenContextMenu);
            }
        }

        [RelayCommand]
        private void ShiftEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null)
            {
                return;
            }

            entryActionService.RunAction(entry.SearchResult, ActionType.CopyUserName);
        }

        [RelayCommand]
        private void ControlEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null)
            {
                return;
            }

            entryActionService.RunAction(entry.SearchResult, ActionType.CopyPassword);
        }

        [RelayCommand]
        private void AltEnterAction(EntryViewModel entryViewModel)
        {
            var entry = entryViewModel ?? SelectedEntry;
            if (entry == null)
            {
                return;
            }

            entryActionService.RunAction(entry.SearchResult, ActionType.CopyTotp);
        }

        [RelayCommand]
        private void RunAction(ActionType actionType)
        {
            if (SelectedEntry == null)
            {
                return;
            }

            entryActionService.RunAction(SelectedEntry.SearchResult, actionType);
        }

        [RelayCommand]
        private void NavigateListDown()
        {
            if (IsContextMenuOpen)
            {
                NavigateCollcetionDown(ContextActions, SelectedContextAction, (x) => SelectedContextAction = x);
            }
            else
            {
                NavigateCollcetionDown(Entries, SelectedEntry, (x) => SelectedEntry = x);
            }
        }

        [RelayCommand]
        private void NavigateListUp()
        {
            if(IsContextMenuOpen)
            {
                NavigateCollcetionUp(ContextActions, SelectedContextAction, (x) => SelectedContextAction = x);
            }
            else
            {
                NavigateCollcetionUp(Entries, SelectedEntry, (x) => SelectedEntry = x);
            }
        }

        partial void OnSearchTextChanged(string searchQuery)
        {
            var dbs = pluginProxy.Databases;

            SelectedEntry = null;
            Entries.Clear();

            if (dbs != null)
            {
                var entrySearchResults = entrySearchService.SearchEntries(dbs, searchQuery, pluginProxy.Settings);
                foreach (var entrySearchResult in entrySearchResults)
                {
                    Entries.Add(new EntryViewModel(entrySearchResult, pluginProxy));
                }

                SelectedEntry = Entries.FirstOrDefault();
            }
        }

        private static void NavigateCollcetionDown<T>(ObservableCollection<T> collection, T selectedItem, Action<T> udpateSelectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }

            var selectedIndex = collection.IndexOf(selectedItem);
            var nextItemIndex = selectedIndex + 1;
            if (nextItemIndex < collection.Count)
            {
                udpateSelectedItem(collection[nextItemIndex]);
            }
        }

        private static void NavigateCollcetionUp<T>(ObservableCollection<T> collection, T selectedItem, Action<T> udpateSelectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }

            var selectedIndex = collection.IndexOf(selectedItem);
            var previousItemIndex = selectedIndex - 1;
            if (previousItemIndex >= 0)
            {
                udpateSelectedItem(collection[previousItemIndex]);
            }
        }
    }
}
