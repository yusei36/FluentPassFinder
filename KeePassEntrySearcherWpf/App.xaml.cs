using KeePassEntrySearcherWpf.Views;
using KeePassEntrySearcherWpf.ViewModels;
using NHotkey.Wpf;
using System.Windows.Input;
using NHotkey;
using System.Windows;
using KeePassEntrySearcherContracts;
using KeePassEntrySearcherWpf.Services;

namespace KeePassEntrySearcherWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private SearchWindow searchWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        public void Init(IKeePassDataProvider dataProvider, IKeePassInteractionManager interactionManager)
        {
            searchWindow = new SearchWindow(new SearchWindowViewModel(dataProvider, interactionManager, new EntrySearchService()));
            MainWindow = searchWindow;

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            searchWindow?.ShowSearchWindow();
        }
    }
}
