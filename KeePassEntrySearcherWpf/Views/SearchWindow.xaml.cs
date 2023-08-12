using KeePassEntrySearcherWpf.ViewModels;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace KeePassEntrySearcherWpf.Views
{
    public partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }

        public SearchWindow(
            SearchWindowViewModel viewModel
        )
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            Hide();
            ViewModel.EscapeCommand.Execute(this);
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            SearchBox.Focus();
        }
    }
}
