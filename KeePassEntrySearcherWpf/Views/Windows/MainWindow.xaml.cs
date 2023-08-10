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
            MainWindowViewModel viewModel,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService
        )
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            HotKeyManager.RegisterHotKey(System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

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

        private void HotKeyManager_HotKeyPressed(object? sender, HotKeyEventArgs e)
        {
            Show();
            Activate();
        }
    }
}
