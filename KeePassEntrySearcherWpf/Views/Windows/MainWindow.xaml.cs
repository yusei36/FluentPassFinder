using KeePassEntrySearcher;
using KeePassEntrySearcherWpf.ViewModels.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace KeePassEntrySearcherWpf.Views.Windows
{
    public partial class MainWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel
        )
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            KeyUp += MainWindow_KeyUp;
            Deactivated += MainWindow_Deactivated;

            InitializeComponent();
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            Hide();
        }

        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            AutoSuggestBox.Focus();
        }
    }
}
