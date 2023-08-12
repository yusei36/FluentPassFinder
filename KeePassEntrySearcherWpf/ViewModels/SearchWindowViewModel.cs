using CommunityToolkit.Mvvm.ComponentModel;
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
        private string _applicationTitle = "KeePassEntrySearcherWpf";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<EntryViewModel> _entries = new ObservableCollection<EntryViewModel>();

        public SearchWindowViewModel(IKeePassDataProvider dataProvider, IEntrySearchService entrySearchService)
        {
            this.dataProvider = dataProvider;
            this.entrySearchService = entrySearchService;
        }

        partial void OnSearchTextChanged(string searchQuery)
        {
            var dbs = dataProvider.GetPwDatabases();

            Entries.Clear();

            if (dbs != null)
            {
                var pwEntryResults = entrySearchService.GetPwEntries(dbs, searchQuery);
                foreach (var pwEntry in pwEntryResults)
                {
                    Entries.Add(new EntryViewModel(pwEntry));
                }
            }
        }
    }
}
