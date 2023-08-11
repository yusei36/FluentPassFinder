using CommunityToolkit.Mvvm.ComponentModel;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - KeePassEntrySearcherWpf";

        [ObservableProperty]
        private string _searchText = string.Empty;
    }
}
