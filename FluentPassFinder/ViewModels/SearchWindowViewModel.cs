using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using System.Collections.ObjectModel;

namespace FluentPassFinder.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IPluginHostProxy hostProxy;
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

        public Action HideSearchWindow;
        public Boolean IsAnyDatabaseOpen => hostProxy.GetPwDatabases().Any();

        public SearchWindowViewModel(IPluginHostProxy hostProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService)
        {
            this.hostProxy = hostProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
            contextActions = new ObservableCollection<IAction>(entryActionService.Actions.Where(a => a.ActionType != ActionType.OpenContextMenu));
            selectedContextAction = contextActions.First();
        }

        [RelayCommand]
        private void EnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            if (IsContextMenuOpen)
            {
                if (SelectedContextAction == null)
                {
                    return;
                }

                entryActionService.RunAction(SelectedEntry.SearchResult, SelectedContextAction);
                HideSearchWindow?.Invoke();
            }
            else 
            {

                entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.OpenContextMenu);
            }
        }

        [RelayCommand]
        private void ShiftEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.CopyUserName);
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        private void ControlEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.CopyPassword);
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        private void AltEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.CopyTotp);
            HideSearchWindow?.Invoke();
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
            var dbs = hostProxy.GetPwDatabases();

            SelectedEntry = null;
            Entries.Clear();

            if (dbs != null)
            {
                var entrySearchResults = entrySearchService.SearchEntries(dbs, searchQuery, hostProxy.GetSearchOptions());
                foreach (var entrySearchResult in entrySearchResults)
                {
                    Entries.Add(new EntryViewModel(entrySearchResult, hostProxy));
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
