using FluentPassFinder.ViewModels;
using System.Windows.Controls;

namespace FluentPassFinder.Views
{
    public partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }
        private bool isClosing = false;
        private bool isOpening = false;

        public SearchWindow(SearchWindowViewModel viewModel)
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            HideSearchWindow();
        }

        [RelayCommand]
        public void HideSearchWindow()
        {
            if (!isClosing && !isOpening)
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
                isOpening = true;
                Show();
                Activate();
                isOpening = false;
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
