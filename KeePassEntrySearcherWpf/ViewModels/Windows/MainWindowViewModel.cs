using System.Collections.ObjectModel;

namespace KeePassEntrySearcherWpf.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - KeePassEntrySearcherWpf";

    }
}
