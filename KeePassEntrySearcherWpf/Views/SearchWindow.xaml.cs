using KeePassEntrySearcherWpf.ViewModels;

namespace KeePassEntrySearcherWpf.Views
{
    public partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }

        public SearchWindow(SearchWindowViewModel viewModel)
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            HideSearchWindow();
        }

        [RelayCommand]
        public void HideSearchWindow()
        {
            Hide();
            ViewModel.SearchText = string.Empty;
            ViewModel.Entries.Clear();
        }

        public void ShowSearchWindow()
        {
            Show();
            Activate();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SearchBox.Focus();
        }
    }
}
