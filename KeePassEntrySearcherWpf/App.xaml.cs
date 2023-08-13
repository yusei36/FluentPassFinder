using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using SimpleInjector;
using KeePassEntrySearcherWpf.Views;
using KeePassEntrySearcherWpf.ViewModels;
using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;
using KeePassEntrySearcherWpf.Services;

namespace KeePassEntrySearcherWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private SearchWindow? searchWindow;
        private Container? container;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            container = new Container();
            container.Register<SearchWindow, SearchWindow>();
            container.Register<SearchWindowViewModel, SearchWindowViewModel>();

            container.Register<IEntryActionService, EntryActionService>();
            container.Register<IEntrySearchService, EntrySearchService>();
        }

        public void Init(IKeePassDataProvider dataProvider, IKeePassInteractionManager interactionManager)
        {
            if (dataProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProvider));
            }
            if (interactionManager == null)
            {
                throw new ArgumentNullException(nameof(interactionManager));
            }
            if (container == null)
            {
                throw new NullReferenceException("Container shouldn't be null while initializing");
            }

            container.RegisterInstance(dataProvider);
            container.RegisterInstance(interactionManager);

            searchWindow = container.GetInstance<SearchWindow>();
            MainWindow = searchWindow;

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            searchWindow?.ShowSearchWindow();
        }
    }
}
