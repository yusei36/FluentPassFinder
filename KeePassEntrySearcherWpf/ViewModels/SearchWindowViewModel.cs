using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;
using System.Collections.ObjectModel;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        private readonly IKeePassDataProvider dataProvider;
        private readonly IEntrySearchService entrySearchService;

        
        [ObservableProperty]
        private string applicationTitle = "KeePassEntrySearcherWpf";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EntryViewModel> _entries = new ObservableCollection<EntryViewModel>();

        public SearchWindowViewModel(IKeePassDataProvider dataProvider, IEntrySearchService entrySearchService)
        {
            this.dataProvider = dataProvider;
            this.entrySearchService = entrySearchService;
        }

        [RelayCommand]
        public void OnEscape(object sender)
        {
            SearchText = string.Empty;
            Entries.Clear();
        }

        partial void OnSearchTextChanged(string searchQuery)
        {
            var dbs = dataProvider.GetPwDatabases();

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
                    Entries.Add(new EntryViewModel(pwEntry));
                }
            }
        }
    }
}
