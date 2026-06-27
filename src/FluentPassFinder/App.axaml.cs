// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Ipc;
using FluentPassFinder.Platform;
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
        private IPlatformServices _platform;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Created up front so the startup-failure paths below can show a message box.
                _platform = new WindowsPlatformServices();

                var args = desktop.Args;
                if (SingleInstance.TryParseActivationAction(args, out var action))
                {
                    if (SingleInstance.TrySignalRunningInstance(action))
                    {
                        desktop.Shutdown(0);
                        return;
                    }

                    _platform.ShowError(
                        "FluentPassFinder is started automatically by the KeePass plugin and "
                        + "cannot run on its own.\n\nOpen KeePass and use the configured hotkey "
                        + "to show the search window.");
                    desktop.Shutdown(1);
                    return;
                }

                int? hostPid = args.Length >= 2 && int.TryParse(args[1], out var pid) ? pid : (int?)null;

                try
                {
                    Init(args[0], hostPid, desktop);
                }
                catch (Exception ex)
                {
                    Program.WriteLog("InitException", ex.ToString());
                    _platform.ShowError($"FluentPassFinder failed to start:\n\n{ex.Message}");
                    desktop.Shutdown(1);
                    return;
                }

                if (hostPid.HasValue)
                    WatchHostProcess(hostPid.Value);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Init(string pipeName, int? hostPid, IClassicDesktopStyleApplicationLifetime desktop)
        {
            _instance = this;

            var pipeClient = new PipeClient(pipeName, hostPid);
            pipeClient.Connect();

            var services = new ServiceCollection();
            services.AddSingleton<IPluginProxy>(pipeClient);
            services.AddSingleton<IPlatformServices>(_platform);
            services.AddSingleton<Lazy<SearchWindow>>(_ => new Lazy<SearchWindow>(() =>
                _searchWindow ?? throw new InvalidOperationException("Current search window is null.")));
            services.AddSingleton<Lazy<SearchWindowViewModel>>(_ => new Lazy<SearchWindowViewModel>(() =>
                _searchWindow?.ViewModel ?? throw new InvalidOperationException("Current search window view model is null.")));
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<CreateEntryViewModel>();
            services.AddSingleton<ISearchWindowInteractionService, SearchWindowInteractionService>();
            services.AddSingleton<IEntrySearchService, EntrySearchService>();
            services.AddSingleton<IEntryActionService, EntryActionService>();
            services.AddSingleton<SearchWindowViewModel>();
            services.AddTransient<IStaticAction, AutoTypeEntryAction>();
            services.AddTransient<IStaticAction, OpenContextMenuAction>();
            services.AddTransient<IStaticAction, OpenUrlAction>();
            services.AddTransient<IStaticAction, SelectEntryAction>();
            services.AddTransient<IStaticAction, CreateFromTemplateAction>();
            services.AddTransient<IStaticAction, CopyTotpAction>();
            services.AddTransient<IStaticAction, AutoTypeTotpAction>();
            services.AddTransient<IFieldAction, AutoTypeAction>();
            services.AddTransient<IFieldAction, CopyAction>();
            var provider = services.BuildServiceProvider();

            var viewModel = provider.GetRequiredService<SearchWindowViewModel>();
            var settingsViewModel = provider.GetRequiredService<SettingsViewModel>();
            var settingsView = new Views.SettingsView(settingsViewModel);
            var createEntryViewModel = provider.GetRequiredService<CreateEntryViewModel>();
            var createEntryView = new Views.CreateEntryView(createEntryViewModel);
            _searchWindow = new SearchWindow(viewModel, settingsView, createEntryView, _platform);

            var settings = pipeClient.Settings;
            RegisterHotkeys(settings);
            RestoreTheme(settings);
            ApplyWindowSize(settings);

            // Render the window once off-screen so the first hotkey press shows a
            // fully composited window instead of an unrendered "skeleton" frame.
            _searchWindow.WarmUp();

            SingleInstance.StartActivationListener(action =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() => Activate(action)));
        }

        private void Activate(ActivationAction action)
        {
            switch (action)
            {
                case ActivationAction.ShowPrimary:
                    _searchWindow?.ShowSearchWindow(true);
                    break;
                case ActivationAction.NewEntry:
                    _searchWindow?.ShowSearchWindow(false, openCreateEntry: true);
                    break;
                default:
                    _searchWindow?.ShowSearchWindow(false);
                    break;
            }
        }

        internal static async Task CopyToClipboardAsync(string text)
        {
            if (_instance?._searchWindow is { } window
                && TopLevel.GetTopLevel(window)?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(text);
        }

        private void WatchHostProcess(int hostPid)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var host = Process.GetProcessById(hostPid);
                    await host.WaitForExitAsync();
                }
                catch { /* process already gone or no longer accessible */ }

                _platform?.DisposeHotkeys();
                Environment.Exit(0);
            });
        }

        internal static void ApplySettings(Settings settings)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Current.RequestedThemeVariant = ToThemeVariant(settings.Theme);
                ApplyWindowSize(settings);
            });

            _instance?.UnregisterHotkeys();
            _instance?.RegisterHotkeys(settings);
        }

        private static void ApplyWindowSize(Settings settings)
        {
            if (_instance?._searchWindow == null) return;
            var defaults = Settings.CreateDefault().Window;
            var width = settings.Window.Width > 0 ? settings.Window.Width : defaults.Width;
            var maxResultsH = settings.Window.Height > 0 ? settings.Window.Height : defaults.Height;
            _instance._searchWindow.Width = width;
            _instance._searchWindow.MaxHeight = SearchWindow.HeaderSize + maxResultsH;
            _instance._searchWindow.RecenterIfVisible();
        }

        private void RegisterHotkeys(Settings settings)
        {
            _platform.RegisterHotkey(
                nameof(HotkeyOptions.PrimaryScreen),
                settings.Hotkeys.PrimaryScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _searchWindow?.ShowSearchWindow(true)));

            _platform.RegisterHotkey(
                nameof(HotkeyOptions.CurrentScreen),
                settings.Hotkeys.CurrentScreen,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _searchWindow?.ShowSearchWindow(false)));

            _platform.RegisterHotkey(
                nameof(HotkeyOptions.NewEntry),
                settings.Hotkeys.NewEntry,
                () => Avalonia.Threading.Dispatcher.UIThread.Post(() => _searchWindow?.ShowSearchWindow(false, openCreateEntry: true)));
        }

        private void UnregisterHotkeys()
        {
            _platform.UnregisterHotkey(nameof(HotkeyOptions.PrimaryScreen));
            _platform.UnregisterHotkey(nameof(HotkeyOptions.CurrentScreen));
            _platform.UnregisterHotkey(nameof(HotkeyOptions.NewEntry));
        }

        private void RestoreTheme(Settings settings)
        {
            RequestedThemeVariant = ToThemeVariant(settings.Theme);
        }

        private static ThemeVariant ToThemeVariant(AppTheme theme) => theme switch
        {
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };
    }
}
