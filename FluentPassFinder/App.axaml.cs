using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Ipc;
using FluentPassFinder.Services;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Views;
using SimpleInjector;
using System.Diagnostics;
using System.Reflection;

namespace FluentPassFinder
{
    public partial class App : Application
    {
        public static SimpleInjector.Container Container { get; private set; }

        private static App _instance;
        private SearchWindow _searchWindow;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var args = desktop.Args;
                if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
                {
                    desktop.Shutdown(1);
                    return;
                }

                try
                {
                    Init(args[0], desktop);
                }
                catch (Exception ex)
                {
                    Program.WriteLog("InitException", ex.ToString());
                    desktop.Shutdown(1);
                    return;
                }

                if (args.Length >= 2 && int.TryParse(args[1], out var hostPid))
                    WatchHostProcess(hostPid, desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Init(string pipeName, IClassicDesktopStyleApplicationLifetime desktop)
        {
            _instance = this;

            var pipeClient = new PipeClient(pipeName);
            pipeClient.Connect();

            Container = new SimpleInjector.Container();
            Container.Register<SearchWindowViewModel, SearchWindowViewModel>();
            Container.RegisterSingleton<SettingsViewModel, SettingsViewModel>();
            Container.RegisterSingleton(() => new Lazy<SearchWindowViewModel>(() =>
                _searchWindow?.ViewModel ?? throw new InvalidOperationException("Current search window view model is null.")));
            Container.RegisterSingleton(() => new Lazy<SearchWindow>(() =>
                _searchWindow ?? throw new InvalidOperationException("Current search window is null.")));
            Container.RegisterSingleton<ISearchWindowInteractionService, SearchWindowInteractionService>();
            Container.Register<IEntryActionService, EntryActionService>();
            Container.Register<IEntrySearchService, EntrySearchService>();
            Container.Collection.Register<IStaticAction>(Assembly.GetAssembly(typeof(App)));
            Container.Collection.Register<IFieldAction>(Assembly.GetAssembly(typeof(App)));
            Container.RegisterInstance<IPluginProxy>(pipeClient);

            var viewModel = Container.GetInstance<SearchWindowViewModel>();
            var settingsViewModel = Container.GetInstance<SettingsViewModel>();
            var settingsView = new Views.SettingsView(settingsViewModel);
            _searchWindow = new SearchWindow(viewModel, settingsView);

            var settings = pipeClient.Settings;
            RegisterHotkeys(settings);
            RestoreTheme(settings);
        }

        private void WatchHostProcess(int hostPid, IClassicDesktopStyleApplicationLifetime desktop)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var host = Process.GetProcessById(hostPid);
                    await host.WaitForExitAsync();
                }
                catch { /* process already gone */ }

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    desktop.Shutdown(0));
            });
        }

        internal static void ApplySettings(Settings settings)
        {
            var isDark = !string.Equals(settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                Current.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light);

            HotkeyRegistrar.Unregister(nameof(Settings.GlobalHotkeyPrimaryScreen));
            HotkeyRegistrar.Unregister(nameof(Settings.GlobalHotkeyCurrentScreen));
            HotkeyRegistrar.Register(
                nameof(Settings.GlobalHotkeyPrimaryScreen),
                settings.GlobalHotkeyPrimaryScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _instance?._searchWindow?.ShowSearchWindow(true)));
            HotkeyRegistrar.Register(
                nameof(Settings.GlobalHotkeyCurrentScreen),
                settings.GlobalHotkeyCurrentScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _instance?._searchWindow?.ShowSearchWindow(false)));
        }

        private void RegisterHotkeys(Settings settings)
        {
            HotkeyRegistrar.Register(
                nameof(Settings.GlobalHotkeyPrimaryScreen),
                settings.GlobalHotkeyPrimaryScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _searchWindow?.ShowSearchWindow(true)));

            HotkeyRegistrar.Register(
                nameof(Settings.GlobalHotkeyCurrentScreen),
                settings.GlobalHotkeyCurrentScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _searchWindow?.ShowSearchWindow(false)));
        }

        private void RestoreTheme(Settings settings)
        {
            var isDark = !string.Equals(settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
            RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
