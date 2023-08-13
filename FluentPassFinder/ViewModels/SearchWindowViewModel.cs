using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;
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
        private EntryViewModel? selectedEntry;

        public Action? HideSearchWindow;
        public Boolean IsAnyDatabaseOpen => hostProxy.GetPwDatabases().Any();

        public SearchWindowViewModel(IPluginHostProxy hostProxy, IEntrySearchService entrySearchService, IEntryActionService entryActionService)
        {
            this.hostProxy = hostProxy;
            this.entrySearchService = entrySearchService;
            this.entryActionService = entryActionService;
        }

        [RelayCommand]
        private void EnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.CopyUserName);
            HideSearchWindow?.Invoke();
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

            entryActionService.RunAction(SelectedEntry.SearchResult, ActionType.CopyUserName);
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        private void NavigateListDown()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            var selectedIndex = Entries.IndexOf(SelectedEntry);
            var nextItemIndex = selectedIndex + 1;
            if (nextItemIndex < Entries.Count)
            {
                SelectedEntry = Entries[nextItemIndex];
            }
        }

        [RelayCommand]
        private void NavigateListUp()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            var selectedIndex = Entries.IndexOf(SelectedEntry);
            var previousItemIndex = selectedIndex - 1;
            if (previousItemIndex >= 0)
            {
                SelectedEntry = Entries[previousItemIndex];
            }
        }

        partial void OnSearchTextChanged(string searchQuery)
        {
            var dbs = hostProxy.GetPwDatabases();

            SelectedEntry = null;
            Entries.Clear();

            if (dbs != null)
            {
                var defaultOptions = new SearchOptions()
                {
                    IncludeTitleFiled = true,
                    IncludeNotesField = true,
                    IncludeUrlField = true,
                    IncludeCustomFields = true
                };
                var entrySearchResults = entrySearchService.SearchEntries(dbs, searchQuery, defaultOptions);
                foreach (var entrySearchResult in entrySearchResults)
                {
                    Entries.Add(new EntryViewModel(entrySearchResult, hostProxy));
                }

                SelectedEntry = Entries.FirstOrDefault();
            }
        }
    }
}
