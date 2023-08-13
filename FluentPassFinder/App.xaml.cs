using FluentPassFinder.Services;
using FluentPassFinder.Services.Actions;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Views;
using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;
using NHotkey;
using NHotkey.Wpf;
using SimpleInjector;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace FluentPassFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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

        public void Init(IPluginHostProxy interactionManager)
        {
            if (interactionManager == null)
            {
                throw new ArgumentNullException(nameof(interactionManager));
            }
            if (Container == null)
            {
                throw new NullReferenceException("Container shouldn't be null while initializing");
            }

            Container.RegisterInstance(interactionManager);

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        public static Container? Container { get; private set; }

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            if (Container == null)
            {
                throw new NullReferenceException("Container is null");
            }

            var searchWindow = Container.GetInstance<SearchWindow>();
            MainWindow = searchWindow;
            searchWindow?.ShowSearchWindow();
        }
    }
}
