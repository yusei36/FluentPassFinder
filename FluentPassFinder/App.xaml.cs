using FluentPassFinder.Contracts;
using FluentPassFinder.Services;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Views;
using FluentPassFinderContracts;
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
            if (e.Args != null && e.Args.Any() && e.Args[0].Equals("-standalone", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("Standalone mode isn't implemented.", "Error", MessageBoxButton.OK);
                Shutdown(1);
                return;
            }

            var stackTrace = Environment.StackTrace;
            if (!stackTrace.EndsWith("System.Threading.ThreadHelper.ThreadStart()", StringComparison.InvariantCultureIgnoreCase))
            { 
                MessageBox.Show("Can't be run standalone.", "Error", MessageBoxButton.OK);
                Shutdown(1);
                return;
            }

            base.OnStartup(e);
        }

        public void Init(IPluginProxy interactionManager)
        {
            if (interactionManager == null)
            {
                throw new ArgumentNullException(nameof(interactionManager));
            }

            Container = new Container();
            Container.Register<SearchWindowViewModel, SearchWindowViewModel>();
            Container.Register(() => new Lazy<SearchWindowViewModel>(() => searchWindow?.ViewModel ?? throw new ArgumentNullException("Current search window view model is null.")));
            Container.Register(() => new Lazy<SearchWindow>(() => searchWindow ?? throw new ArgumentNullException("Current search window view model is null.")));
            Container.Register<ISearchWindowInteractionService, SearchWindowInteractionService>();

            Container.Register<IEntryActionService, EntryActionService>();
            Container.Register<IEntrySearchService, EntrySearchService>();
            Container.Collection.Register<IAction>(Assembly.GetAssembly(typeof(App)));

            Container.RegisterInstance(interactionManager);

            var viewModel = Container.GetInstance<SearchWindowViewModel>();
            searchWindow = new SearchWindow(viewModel);
            MainWindow = searchWindow;

            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow)+ nameof(Key.F), Key.F, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
            HotkeyManager.Current.AddOrReplace(nameof(ShowSearchWindow)+ nameof(Key.S), Key.S, ModifierKeys.Control | ModifierKeys.Alt, ShowSearchWindow);
        }

        public static Container Container { get; private set; }

        private SearchWindow searchWindow;

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            if (Container == null)
            {
                throw new NullReferenceException("Container is null");
            }
            
            Dispatcher.BeginInvoke(()=> searchWindow?.ShowSearchWindow());
        }
    }
}
