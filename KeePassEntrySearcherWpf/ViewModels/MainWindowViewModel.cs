using CommunityToolkit.Mvvm.ComponentModel;
using KeePassEntrySearcherContracts;
using KeePassLib;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IKeePassDataProvider dataProvider;

        [ObservableProperty]
        private string _applicationTitle = "WPF UI - KeePassEntrySearcherWpf";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private PwEntry[] _results = new PwEntry[0];

        public MainWindowViewModel(IKeePassDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        partial void OnSearchTextChanged(string value)
        {
            var dbs = dataProvider.GetPwDatabases();
            if (dbs != null)
            {
                Results = dbs.SelectMany(db => db.RootGroup.GetEntries(true)).ToArray();
            }
            else
            {
                Results = new PwEntry[0];
            }
        }
    }
}
