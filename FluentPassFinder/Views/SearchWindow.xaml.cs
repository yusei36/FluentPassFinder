using FluentPassFinder.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FluentPassFinder.Views
{
    public partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }
        private bool isClosing = false;

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
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            HideSearchWindow();
        }

        [RelayCommand]
        public void HideSearchWindow()
        {
            if (!isClosing)
            {
                isClosing = true;
                Close();

                ViewModel.SearchText = string.Empty;
                ViewModel.Entries.Clear();
            }
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
