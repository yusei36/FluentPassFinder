using KeePassEntrySearcherWpf.Views;
using KeePassEntrySearcherWpf.ViewModels;
using NHotkey.Wpf;
using System.Windows.Input;
using NHotkey;
using System.Windows;

namespace KeePassEntrySearcherWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            MainWindow = new MainWindow(new MainWindowViewModel());

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            MainWindow.Show();
            MainWindow.Activate();
        }
    }
}
