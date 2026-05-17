// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using FluentPassFinder.Contracts;
using FluentPassFinder.ViewModels;
using System.Runtime.InteropServices;

namespace FluentPassFinder.Views
{
    internal partial class SearchWindow : Window
    {
        public SearchWindowViewModel ViewModel { get; }
        public SettingsView SettingsView { get; }
        public static double HeaderSize = 40.0;

        private bool _isClosing;
        private bool _isOpening;
        private bool _pendingHide;

        public SearchWindow() { InitializeComponent(); }

        public SearchWindow(SearchWindowViewModel viewModel, SettingsView settingsView)
        {
            ViewModel = viewModel;
            SettingsView = settingsView;
            DataContext = this;

            InitializeComponent();

            Deactivated += (_, _) => HideSearchWindow();
        }

        public void FocusSearchBox() => SearchBox.Focus();

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void HideSearchWindow()
        {
            if (_isClosing || _isOpening) return;

            _isClosing = true;
            _pendingHide = true;

            ViewModel.SearchText = string.Empty;
            SearchBox.Text = string.Empty;
            ViewModel.ClearEntries();
            ViewModel.IsContextMenuOpen = false;
            ViewModel.IsSettingsOpen = false;
            ViewModel.SelectedEntry = null;

            // Defer Hide() so Avalonia renders one frame with the cleared state first.
            // That way the GPU frame buffer is already empty when the window is next shown.
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _isClosing = false;
                if (_pendingHide)
                {
                    _pendingHide = false;
                    Hide();
                }
            }, Avalonia.Threading.DispatcherPriority.Background);
        }

        public void ShowSearchWindow(bool showOnPrimaryScreen)
        {
            if (!ViewModel.IsAnyDatabaseOpen) return;

            _pendingHide = false; // cancel any deferred hide
            _isOpening = true;

            ViewModel.SearchText = string.Empty;
            ViewModel.IsContextMenuOpen = false;
            ViewModel.IsSettingsOpen = false;
            SearchBox.Text = string.Empty;

            SetCenteredWindowPosition(showOnPrimaryScreen);
            Show();
            Activate();
            SearchBox.Focus();

            _isOpening = false;
        }

        private void SetCenteredWindowPosition(bool showOnPrimaryScreen)
        {
            var screens = Screens;
            Screen screen;
            if (showOnPrimaryScreen)
            {
                screen = screens.Primary;
            }
            else
            {
                var cursorPos = GetCursorPixelPos();
                screen = screens.ScreenFromPoint(cursorPos) ?? screens.Primary;
            }

            if (screen == null) return;

            var wa = screen.WorkingArea; // physical pixels
            double scaling = screen.Scaling;

            int x = wa.X + (int)((wa.Width - Width * scaling) / 2.0);
            int y = wa.Y + (int)((wa.Height - HeaderSize * scaling) / 2.0) - (int)(HeaderSize * scaling);

            Position = new PixelPoint(x, y);
        }

        private void ClearSearchButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void EntryGrid_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;

            ViewModel.SelectedEntry = (sender as Control)?.DataContext as EntryViewModel;
            var mods = e.KeyModifiers;

            if (mods.HasFlag(KeyModifiers.Shift))
                ViewModel.ShiftEnterActionCommand.Execute(null);
            else if (mods.HasFlag(KeyModifiers.Control))
                ViewModel.ControlEnterActionCommand.Execute(null);
            else if (mods.HasFlag(KeyModifiers.Alt))
                ViewModel.AltEnterActionCommand.Execute(null);
            else
                ViewModel.EnterActionCommand.Execute(null);
        }

        private void ActionPanel_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
                ViewModel.RunActionCommand.Execute(((sender as Control)?.DataContext as IAction)?.ActionType);
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private static PixelPoint GetCursorPixelPos()
        {
            GetCursorPos(out var pt);
            return new PixelPoint(pt.X, pt.Y);
        }
    }
}
