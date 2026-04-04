using FluentPassFinder.Contracts;
using FluentPassFinder.Ipc;
using FluentPassFinder.Services;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Views;
using FluentPassFinder.Contracts.Public;
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
            if (e.Args == null || e.Args.Length == 0 || string.IsNullOrEmpty(e.Args[0]))
            {
                MessageBox.Show("This application is launched by the FluentPassFinder KeePass plugin.", "Error", MessageBoxButton.OK);
                Shutdown(1);
                return;
            }

            base.OnStartup(e);
            Init(e.Args[0]);
        }

        private void Init(string pipeName)
        {
            if (string.IsNullOrEmpty(pipeName))
                throw new ArgumentNullException(nameof(pipeName));

            var pipeClient = new PipeClient(pipeName);
            pipeClient.Connect();

            Container = new Container();
            Container.Register<SearchWindowViewModel, SearchWindowViewModel>();
            Container.Register(() => new Lazy<SearchWindowViewModel>(() => searchWindow?.ViewModel ?? throw new ArgumentNullException("Current search window view model is null.")));
            Container.Register(() => new Lazy<SearchWindow>(() => searchWindow ?? throw new ArgumentNullException("Current search window view model is null.")));
            Container.Register<ISearchWindowInteractionService, SearchWindowInteractionService>();

            Container.Register<IEntryActionService, EntryActionService>();
            Container.Register<IEntrySearchService, EntrySearchService>();
            Container.Collection.Register<IStaticAction>(Assembly.GetAssembly(typeof(App)));
            Container.Collection.Register<IFieldAction>(Assembly.GetAssembly(typeof(App)));

            Container.RegisterInstance<IPluginProxy>(pipeClient);

            var viewModel = Container.GetInstance<SearchWindowViewModel>();
            searchWindow = new SearchWindow(viewModel);
            MainWindow = searchWindow;

            var settings = pipeClient.Settings;
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
