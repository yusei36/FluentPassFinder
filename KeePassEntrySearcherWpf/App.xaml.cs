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
using System.Reflection;
using KeePassEntrySearcherWpf.Services.Actions;

namespace KeePassEntrySearcherWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private SearchWindow? searchWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Container = new Container();
            Container.Register<SearchWindow, SearchWindow>();
            Container.Register<SearchWindowViewModel, SearchWindowViewModel>();

            Container.Register<IEntryActionService, EntryActionService>(Lifestyle.Singleton);
            Container.Register<IEntrySearchService, EntrySearchService>(Lifestyle.Singleton);
            Container.Collection.Register<IAction>(Assembly.GetAssembly(typeof(App)));
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
            if (Container == null)
            {
                throw new NullReferenceException("Container shouldn't be null while initializing");
            }

            Container.RegisterInstance(dataProvider);
            Container.RegisterInstance(interactionManager);

            searchWindow = Container.GetInstance<SearchWindow>();
            MainWindow = searchWindow;

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        public static Container? Container { get; private set; }

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            searchWindow?.ShowSearchWindow();
        }
    }
}
