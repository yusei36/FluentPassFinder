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
            Container.Collection.Register<IStaticAction>(Assembly.GetAssembly(typeof(App)));
            Container.Collection.Register<IFieldAction>(Assembly.GetAssembly(typeof(App)));

            Container.RegisterInstance(interactionManager);

            var viewModel = Container.GetInstance<SearchWindowViewModel>();
            searchWindow = new SearchWindow(viewModel);
            MainWindow = searchWindow;

            var settings = interactionManager.Settings;
            var converter = new KeyGestureConverter();

            HotkeyManager.Current.AddOrReplace(nameof(Settings.GlobalHotkeyPrimaryScreen), (KeyGesture)converter.ConvertFromInvariantString(settings.GlobalHotkeyPrimaryScreen), ShowSearchWindow);
            HotkeyManager.Current.AddOrReplace(nameof(Settings.GlobalHotkeyCurrentScreen), (KeyGesture)converter.ConvertFromInvariantString(settings.GlobalHotkeyCurrentScreen), ShowSearchWindow);

            RestoreTheme(settings);
        }

        public static Container Container { get; private set; }

        private SearchWindow searchWindow;

        private void ShowSearchWindow(object sender, HotkeyEventArgs e)
        {
            if (Container == null)
            {
                throw new NullReferenceException("Container is null");
            }
            if (e.Name == nameof(Settings.GlobalHotkeyPrimaryScreen))
            {
                searchWindow?.ShowSearchWindow(true);
            }
            else
            {
                searchWindow?.ShowSearchWindow(false);
            }
        }

        private static void RestoreTheme(Settings settings)
        {
            if (Enum.TryParse(settings.Theme, true, out Wpf.Ui.Appearance.ApplicationTheme themeType))
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(themeType);
            }
            else
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
            }
        }
    }
}
