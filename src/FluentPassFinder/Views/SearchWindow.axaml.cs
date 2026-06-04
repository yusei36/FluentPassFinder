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
        private bool _isWarmingUp;
        private bool _pendingHide;
        private readonly Timer _preserveTimer;
        private Timer _warmUpTimer;

        private bool _isBottomAnchor;
        private int _targetHeaderTopY;
        private double _anchorScaling = 1.0;

        private WinEventDelegate _foregroundWatchProc;
        private IntPtr _foregroundWatchHook;

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

            Deactivated += (_, _) => { if (!_isWarmingUp) HideSearchWindow(); };
            SizeChanged += OnWindowSizeChanged;
        }

        public void FocusSearchBox() => SearchBox.Focus();

        /// <summary>
        /// Windows 11 draws a 1px border around every window (following the corner
        /// radius), which shows up even though we render our own borderless rounded
        /// surface. Setting the DWM border color to <c>DWMWA_COLOR_NONE</c> removes it.
        /// The call is a no-op on Windows 10 (the attribute is unsupported there).
        /// </summary>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var hWnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hWnd == IntPtr.Zero) return;

            int colorNone = unchecked((int)DWMWA_COLOR_NONE);
            DwmSetWindowAttribute(hWnd, DWMWA_BORDER_COLOR, ref colorNone, sizeof(int));
        }

        /// <summary>
        /// Renders the window once off-screen at startup so the first real
        /// <see cref="ShowSearchWindow"/> doesn't flash an unrendered "skeleton"
        /// (window border with no composited background). The native surface,
        /// GPU swapchain, glyph caches and render resources are all created
        /// lazily on the first <see cref="Window.Show()"/>; doing that work
        /// off-screen ahead of time makes the first visible show instant.
        /// </summary>
        public void WarmUp()
        {
            _isWarmingUp = true;
            ShowActivated = false; // don't steal focus while warming up
            Position = new PixelPoint(-32000, -32000);
            Show();

            // Keep it shown off-screen briefly so the render thread composites
            // the first frame, then hide it ready for the first real show.
            _warmUpTimer = new Timer(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Hide();
                    ShowActivated = true;
                    _isWarmingUp = false;
                });
            }, null, 200, Timeout.Infinite);
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void HideSearchWindow()
        {
            if (_isClosing || _isOpening || _isWarmingUp) return;

            _isClosing = true;
            _pendingHide = true;

            ViewModel.IsSettingsOpen = false;

            if (ViewModel.Settings.Behavior.PreserveLastSearch && !string.IsNullOrEmpty(ViewModel.SearchText))
            {
                _preserveTimer.Change(ViewModel.Settings.Behavior.PreserveLastSearchTimeoutMilliseconds, Timeout.Infinite);
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

            if (ViewModel.Settings.Behavior.PreserveLastSearch && !string.IsNullOrEmpty(ViewModel.SearchText))
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
            EnsureForegroundWatch();
            var anchor = ViewModel?.Settings?.Window?.Anchor ?? WindowAnchor.CenterCenter;
            ApplyBottomAnchorLayout(anchor.IsBottom());
            ForceForeground();
            Activate();
            SearchBox.Focus();
            _isOpening = false;

        }

        /// <summary>
        /// Forces this window to the foreground and gives it keyboard focus.
        ///
        /// We run as a separate process from the one the user is interacting with
        /// (browser, Explorer, ...). When the hotkey fires, that other process is the
        /// foreground app, so Windows' focus-stealing prevention blocks our
        /// <see cref="Window.Activate"/>/SetForegroundWindow call: the window becomes
        /// visible but keyboard input keeps going to the previously focused app.
        ///
        /// The reliable workaround is to temporarily attach our UI thread's input
        /// queue to the foreground window's thread (<c>AttachThreadInput</c>), which
        /// lifts the restriction so <c>SetForegroundWindow</c> actually takes effect,
        /// then detach again.
        /// </summary>
        private void ForceForeground()
        {
            var hWnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hWnd == IntPtr.Zero) return;

            var foreground = GetForegroundWindow();
            if (foreground == hWnd) return;

            uint foregroundThread = foreground != IntPtr.Zero
                ? GetWindowThreadProcessId(foreground, out _)
                : 0;
            uint currentThread = GetWindowThreadProcessId(hWnd, out _);

            bool attached = foregroundThread != 0
                && foregroundThread != currentThread
                && AttachThreadInput(currentThread, foregroundThread, true);
            try
            {
                SetForegroundWindow(hWnd);
            }
            finally
            {
                if (attached)
                    AttachThreadInput(currentThread, foregroundThread, false);
            }
        }

        /// <summary>
        /// Hides the window when the user switches to another application.
        ///
        /// Relying on Avalonia's <see cref="InputElement.LostFocus"/>/Deactivated alone
        /// is unreliable for a borderless, topmost launcher window: Windows does not
        /// always deliver a deactivation when focus jumps to another process (clicking
        /// the desktop, taskbar, another topmost window, ...), so the window can stay
        /// open. A system-wide <c>EVENT_SYSTEM_FOREGROUND</c> hook fires whenever the
        /// foreground window changes; we hide as soon as it moves to a window owned by
        /// a different process. The process check keeps our own in-process popups
        /// (context-menu flyout, settings pane) from closing the window.
        ///
        /// The hook is installed on the UI thread, so its callback is marshalled back
        /// onto the UI thread's message loop. It is installed once and lives for the
        /// window's lifetime (the process is terminated when KeePass closes).
        /// </summary>
        private void EnsureForegroundWatch()
        {
            if (_foregroundWatchHook != IntPtr.Zero) return;

            _foregroundWatchProc = OnForegroundChanged; // keep a reference so it is not GC'd
            _foregroundWatchHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _foregroundWatchProc, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private void OnForegroundChanged(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;
            if (!IsVisible || _isWarmingUp || _isOpening || _isClosing) return;

            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == (uint)Environment.ProcessId) return; // our own window or popups

            HideSearchWindow();
        }

        public void RecenterIfVisible()
        {
            if (!IsVisible) return;
            var screen = Screens.ScreenFromPoint(Position) ?? Screens.Primary;
            if (screen == null) return;
            var window = ViewModel?.Settings?.Window;
            var anchor = window?.Anchor ?? WindowAnchor.CenterCenter;
            var (hFrac, vFrac) = ParseAnchor(anchor);
            bool isBottom = anchor.IsBottom();
            ApplyBottomAnchorLayout(isBottom);
            var wa = screen.WorkingArea;
            double scaling = screen.Scaling;
            int x = wa.X + (int)(hFrac * (wa.Width - Width * scaling)) + (window?.OffsetX ?? 0);
            int y = wa.Y + (int)(vFrac * (wa.Height - HeaderSize * scaling)) + (window?.OffsetY ?? 0);
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

            var window = ViewModel?.Settings?.Window;
            var anchor = window?.Anchor ?? WindowAnchor.CenterCenter;
            var (hFrac, vFrac) = ParseAnchor(anchor);
            var wa = screen.WorkingArea;
            double scaling = screen.Scaling;

            int x = wa.X + (int)(hFrac * (wa.Width - Width * scaling)) + (window?.OffsetX ?? 0);
            int y = wa.Y + (int)(vFrac * (wa.Height - HeaderSize * scaling)) + (window?.OffsetY ?? 0);

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

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_BORDER_COLOR = 34;
        private const uint DWMWA_COLOR_NONE = 0xFFFFFFFE;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        private delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        private static PixelPoint GetCursorPixelPos()
        {
            GetCursorPos(out var pt);
            return new PixelPoint(pt.X, pt.Y);
        }
    }
}
