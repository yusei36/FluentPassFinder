// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
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
        private readonly Timer _preserveTimer;

        private bool _isBottomAnchor;
        private int _targetHeaderTopY;
        private double _anchorScaling = 1.0;

        public SearchWindow() { InitializeComponent(); }

        public SearchWindow(SearchWindowViewModel viewModel, SettingsView settingsView)
        {
            ViewModel = viewModel;
            SettingsView = settingsView;
            DataContext = this;

            _preserveTimer = new Timer(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (!IsVisible && !_isOpening)
                        ViewModel.SearchText = string.Empty;
                });
            }, null, Timeout.Infinite, Timeout.Infinite);

            InitializeComponent();

            Deactivated += (_, _) => HideSearchWindow();
            SizeChanged += OnWindowSizeChanged;
        }

        public void FocusSearchBox() => SearchBox.Focus();

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void HideSearchWindow()
        {
            if (_isClosing || _isOpening) return;

            _isClosing = true;
            _pendingHide = true;

            ViewModel.IsSettingsOpen = false;

            if (ViewModel.Settings.PreserveLastSearch && !string.IsNullOrEmpty(ViewModel.SearchText))
            {
                _preserveTimer.Change(ViewModel.Settings.PreserveLastSearchTimeoutMilliseconds, Timeout.Infinite);
            }
            else
            {
                _preserveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                ViewModel.IsContextMenuOpen = false;
                ViewModel.SearchText = string.Empty;
                SearchBox.Text = string.Empty;
                ViewModel.ClearEntries();
                ViewModel.SelectedEntry = null;
            }

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

            ViewModel.IsSettingsOpen = false;

            if (ViewModel.Settings.PreserveLastSearch && !string.IsNullOrEmpty(ViewModel.SearchText))
            {
                _preserveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                ViewModel.IsContextMenuOpen = false;
                ViewModel.SearchText = string.Empty;
                SearchBox.Text = string.Empty;
            }

            SetWindowPosition(showOnPrimaryScreen);
            Show();
            var anchor = ViewModel?.Settings?.WindowAnchor ?? WindowAnchor.CenterCenter;
            ApplyBottomAnchorLayout(anchor.IsBottom());
            Activate();
            SearchBox.Focus();
            _isOpening = false;

        }

        public void RecenterIfVisible()
        {
            if (!IsVisible) return;
            var screen = Screens.ScreenFromPoint(Position) ?? Screens.Primary;
            if (screen == null) return;
            var settings = ViewModel?.Settings;
            var anchor = settings?.WindowAnchor ?? WindowAnchor.CenterCenter;
            var (hFrac, vFrac) = ParseAnchor(anchor);
            bool isBottom = anchor.IsBottom();
            ApplyBottomAnchorLayout(isBottom);
            var wa = screen.WorkingArea;
            double scaling = screen.Scaling;
            int x = wa.X + (int)(hFrac * (wa.Width - Width * scaling)) + (settings?.WindowOffsetX ?? 0);
            int y = wa.Y + (int)(vFrac * (wa.Height - HeaderSize * scaling)) + (settings?.WindowOffsetY ?? 0);
            if (isBottom)
            {
                _targetHeaderTopY = y;
                _anchorScaling = scaling;
                y -= (int)((Bounds.Height - HeaderSize) * scaling);
            }
            Position = new PixelPoint(x, y);
        }

        private void SetWindowPosition(bool showOnPrimaryScreen)
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

            var settings = ViewModel?.Settings;
            var anchor = settings?.WindowAnchor ?? WindowAnchor.CenterCenter;
            var (hFrac, vFrac) = ParseAnchor(anchor);
            var wa = screen.WorkingArea;
            double scaling = screen.Scaling;

            int x = wa.X + (int)(hFrac * (wa.Width - Width * scaling)) + (settings?.WindowOffsetX ?? 0);
            int y = wa.Y + (int)(vFrac * (wa.Height - HeaderSize * scaling)) + (settings?.WindowOffsetY ?? 0);

            bool isBottom = anchor.IsBottom();
            if (isBottom)
            {
                _targetHeaderTopY = y;
                _anchorScaling = scaling;
            }

            Position = new PixelPoint(x, y);
        }

        private void ApplyBottomAnchorLayout(bool isBottom)
        {
            if (isBottom == _isBottomAnchor) return;
            _isBottomAnchor = isBottom;
            if (isBottom)
            {
                MainGrid.RowDefinitions[0].Height = GridLength.Star;
                MainGrid.RowDefinitions[1].Height = new GridLength(HeaderSize);
                Grid.SetRow(HeaderPanel, 1);
                Grid.SetRow(SettingsContent, 0);
                Grid.SetRow(ResultsPanel, 0);
            }
            else
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(HeaderSize);
                MainGrid.RowDefinitions[1].Height = GridLength.Star;
                Grid.SetRow(HeaderPanel, 0);
                Grid.SetRow(SettingsContent, 1);
                Grid.SetRow(ResultsPanel, 1);
            }
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isBottomAnchor || !IsVisible || _isClosing) return;
            int newY = _targetHeaderTopY - (int)((Bounds.Height - HeaderSize) * _anchorScaling);
            if (Position.Y != newY)
                Position = new PixelPoint(Position.X, newY);
        }

        private static (double h, double v) ParseAnchor(WindowAnchor anchor) => anchor switch
        {
            WindowAnchor.LeftTop      => (0.0, 0.0),
            WindowAnchor.CenterTop    => (0.5, 0.0),
            WindowAnchor.RightTop     => (1.0, 0.0),
            WindowAnchor.LeftCenter   => (0.0, 0.5),
            WindowAnchor.CenterCenter => (0.5, 0.5),
            WindowAnchor.RightCenter  => (1.0, 0.5),
            WindowAnchor.LeftBottom   => (0.0, 1.0),
            WindowAnchor.CenterBottom => (0.5, 1.0),
            WindowAnchor.RightBottom  => (1.0, 1.0),
            _                         => (0.5, 0.5),
        };

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
