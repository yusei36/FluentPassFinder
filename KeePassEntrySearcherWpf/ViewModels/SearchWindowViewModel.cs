using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;
using System.Collections.ObjectModel;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IKeePassDataProvider dataProvider;
        private readonly IKeePassInteractionManager interactionManager;
        private readonly IEntrySearchService entrySearchService;

        
        [ObservableProperty]
        private string applicationTitle = "KeePassEntrySearcherWpf";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EntryViewModel> entries = new ObservableCollection<EntryViewModel>();

        [ObservableProperty]
        private EntryViewModel? selectedEntry;

        public Action? HideSearchWindow;
        public Boolean IsAnyDatabaseOpen => dataProvider.GetPwDatabases().Any();

        public SearchWindowViewModel(IKeePassDataProvider dataProvider, IKeePassInteractionManager interactionManager, IEntrySearchService entrySearchService)
        {
            this.dataProvider = dataProvider;
            this.interactionManager = interactionManager;
            this.entrySearchService = entrySearchService;
        }

        [RelayCommand]
        public void EnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            //SelectedEntry.CopyUserName();
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        public void ShiftEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            SelectedEntry.CopyUserName();
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        public void ControlEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            SelectedEntry.CopyPassword();
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        public void AltEnterAction()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            //SelectedEntry.CopyUserName();
            HideSearchWindow?.Invoke();
        }

        [RelayCommand]
        public void NavigateListDown()
        {
            if (SelectedEntry == null) 
            { 
                return; 
            }

            var selectedIndex = Entries.IndexOf(SelectedEntry);
            var nextItemIndex = selectedIndex+1;
            if (nextItemIndex < Entries.Count)
            {
                SelectedEntry = Entries[nextItemIndex];
            }
        }

        [RelayCommand]
        public void NavigateListUp()
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
            var dbs = dataProvider.GetPwDatabases();

            SelectedEntry = null;
            Entries.Clear();

            if (dbs != null)
            {
                var defaultOptions = new SearchOptions() {
                    IncludeTitleFiled = true,
                    IncludeNotesField = true,
                    IncludeUrlField = true,
                    IncludeCustomFields = true 
                };
                var pwEntryResults = entrySearchService.GetPwEntries(dbs, searchQuery, defaultOptions);
                foreach (var pwEntry in pwEntryResults)
                {
                    Entries.Add(new EntryViewModel(pwEntry, interactionManager));
                }

                SelectedEntry = Entries.FirstOrDefault();
            }
        }
    }
}
