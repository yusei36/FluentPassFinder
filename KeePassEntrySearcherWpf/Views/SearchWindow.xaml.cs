using KeePassEntrySearcherWpf.ViewModels;
using System.Windows.Controls;

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
            ViewModel.HideSearchWindow = HideSearchWindow;

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
            if (ViewModel.IsAnyDatabaseOpen)
            {
                Show();
                Activate();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SearchBox.Focus();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }
    }
}
