// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Ipc;
using FluentPassFinder.Services;
using FluentPassFinder.Services.Actions.FieldActions;
using FluentPassFinder.Services.Actions.StaticActions;
using FluentPassFinder.ViewModels;
using FluentPassFinder.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace FluentPassFinder
{
    public partial class App : Application
    {
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

            var services = new ServiceCollection();
            services.AddSingleton<IPluginProxy>(pipeClient);
            services.AddSingleton<Lazy<SearchWindow>>(_ => new Lazy<SearchWindow>(() =>
                _searchWindow ?? throw new InvalidOperationException("Current search window is null.")));
            services.AddSingleton<Lazy<SearchWindowViewModel>>(_ => new Lazy<SearchWindowViewModel>(() =>
                _searchWindow?.ViewModel ?? throw new InvalidOperationException("Current search window view model is null.")));
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ISearchWindowInteractionService, SearchWindowInteractionService>();
            services.AddSingleton<IEntrySearchService, EntrySearchService>();
            services.AddSingleton<IEntryActionService, EntryActionService>();
            services.AddSingleton<SearchWindowViewModel>();
            services.AddTransient<IStaticAction, AutoTypeEntryAction>();
            services.AddTransient<IStaticAction, OpenContextMenuAction>();
            services.AddTransient<IStaticAction, OpenUrlAction>();
            services.AddTransient<IStaticAction, SelectEntryAction>();
            services.AddTransient<IStaticAction, CopyTotpAction>();
            services.AddTransient<IStaticAction, AutoTypeTotpAction>();
            services.AddTransient<IFieldAction, AutoTypeAction>();
            services.AddTransient<IFieldAction, CopyAction>();
            var provider = services.BuildServiceProvider();

            var viewModel = provider.GetRequiredService<SearchWindowViewModel>();
            var settingsViewModel = provider.GetRequiredService<SettingsViewModel>();
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
