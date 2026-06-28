// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using FluentPassFinder.Platform;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Views
{
    internal partial class SearchWindow : Window
    {
        public SearchWindowViewModel ViewModel { get; }
        public SettingsView SettingsView { get; }
        public CreateEntryView CreateEntryView { get; }
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

        // Ctrl+drag window move. Armed on a Ctrl+left press, promoted to an actual
        // move once the cursor passes a small threshold so a plain Ctrl+click still
        // falls through to the list/action handlers.
        // Ctrl+drag that starts near the right/bottom edge resizes instead of moving,
        // syncing Width / results-area Height back into the settings view model.
        private const double DragThreshold = 4.0;
        private const double ResizeEdgeMargin = 16.0;
        private const double MinWindowWidth = 200, MaxWindowWidth = 4000;
        private const double MinResultsHeight = 50, MaxResultsHeight = 4000;
        private bool _dragArmed;
        private bool _dragging;
        private PixelPoint _dragStartCursor;
        private PixelPoint _dragStartWindow;

        private bool _resizing;
        private bool _resizeRight;
        private bool _resizeBottom;
        private double _resizeStartWidth;
        private double _resizeStartResultsHeight;

        private readonly IPlatformServices _platform;

        public SearchWindow() { InitializeComponent(); }

        public SearchWindow(SearchWindowViewModel viewModel, SettingsView settingsView, CreateEntryView createEntryView, IPlatformServices platform)
        {
            ViewModel = viewModel;
            SettingsView = settingsView;
            CreateEntryView = createEntryView;
            _platform = platform;
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

            // Focus the Title field when the create-entry overlay opens.
            ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SearchWindowViewModel.IsCreateEntryOpen) && ViewModel.IsCreateEntryOpen)
                    CreateEntryView.FocusFirstField();
            };

            Deactivated += (_, _) => { if (!_isWarmingUp && !ViewModel.IsPinned) HideSearchWindow(); };
            SizeChanged += OnWindowSizeChanged;

            // Tunnel so we observe the gesture before child controls, without consuming
            // the press (children still react to plain Ctrl+clicks).
            AddHandler(PointerPressedEvent, OnDragPointerPressed, RoutingStrategies.Tunnel);
            AddHandler(PointerMovedEvent, OnDragPointerMoved, RoutingStrategies.Tunnel);
            AddHandler(PointerReleasedEvent, OnDragPointerReleased, RoutingStrategies.Tunnel);
        }

        public void FocusSearchBox() => SearchBox.Focus();

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
            ViewModel.IsCreateEntryOpen = false;

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

        public void ShowSearchWindow(bool showOnPrimaryScreen, bool openCreateEntry = false)
        {
            if (!ViewModel.IsAnyDatabaseOpen) return;

            _pendingHide = false; // cancel any deferred hide
            _isOpening = true;
            ViewModel.IsPinned = false; // each fresh show starts unpinned

            ViewModel.IsSettingsOpen = false;
            ViewModel.IsCreateEntryOpen = false;

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

            if (openCreateEntry)
                ViewModel.OpenCreateEntry();

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
        /// Forces this window to the foreground and gives it keyboard focus, working
        /// around the OS focus-stealing prevention (see
        /// <see cref="IPlatformServices.ForceForegroundWindow"/>).
        /// </summary>
        private void ForceForeground()
        {
            var hWnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            _platform.ForceForegroundWindow(hWnd);
        }

        /// <summary>
        /// Hides the window when focus moves to another application. Avalonia's
        /// Deactivated/LostFocus alone is unreliable for a borderless, topmost launcher
        /// window, so we ask the platform to watch for the foreground window changing to
        /// one owned by another process and hide when it does. Idempotent; the watch is
        /// installed once and lives for the window's lifetime.
        /// </summary>
        private void EnsureForegroundWatch()
        {
            _platform.StartForegroundWatch(() =>
            {
                if (!IsVisible || _isWarmingUp || _isOpening || _isClosing || ViewModel.IsPinned) return;
                HideSearchWindow();
            });
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
                var cursorPos = _platform.GetCursorPosition();
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
                Grid.SetRow(CreateEntryContent, 0);
                Grid.SetRow(ResultsPanel, 0);
            }
            else
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(HeaderSize);
                MainGrid.RowDefinitions[1].Height = GridLength.Star;
                Grid.SetRow(HeaderPanel, 0);
                Grid.SetRow(SettingsContent, 1);
                Grid.SetRow(CreateEntryContent, 1);
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

        private void OnDragPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (!point.Properties.IsLeftButtonPressed || !e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                _dragArmed = false;
                return;
            }

            _dragArmed = true;
            _dragging = false;
            _dragStartCursor = _platform.GetCursorPosition();
            _dragStartWindow = Position;

            // Decide move vs. resize from where the press lands. Resizing is only offered
            var local = point.Position;
            _resizeRight = local.X >= Bounds.Width - ResizeEdgeMargin;
            _resizeBottom = local.Y >= Bounds.Height - ResizeEdgeMargin;
            _resizing = _resizeRight || _resizeBottom;
            _resizeStartWidth = Width;
            _resizeStartResultsHeight = SettingsView.ViewModel.WindowHeight is decimal h
                ? (double)h
                : MaxHeight - HeaderSize;
        }

        private void OnDragPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_dragArmed) return;

            // Bail if the button was released outside our handlers or Ctrl was let go.
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _dragArmed = false;
                _dragging = false;
                return;
            }

            var cursor = _platform.GetCursorPosition();
            int dx = cursor.X - _dragStartCursor.X;
            int dy = cursor.Y - _dragStartCursor.Y;

            if (!_dragging && Math.Abs(dx) < DragThreshold && Math.Abs(dy) < DragThreshold)
                return;

            _dragging = true;

            if (_resizing)
                ApplyResize(dx, dy);
            else
                ApplyMove(dx, dy);

            e.Handled = true;
        }

        private void ApplyMove(int dx, int dy)
        {
            int newY = _dragStartWindow.Y + dy;
            Position = new PixelPoint(_dragStartWindow.X + dx, newY);

            // Keep the bottom-anchor reference in sync so a later auto-resize doesn't
            // snap the window back to its computed anchor position.
            if (_isBottomAnchor)
                _targetHeaderTopY = newY + (int)((Bounds.Height - HeaderSize) * _anchorScaling);

            // While settings is open, dragging edits the persisted offset (live preview)
            // instead of a throwaway move. Saving settings then keeps the new placement.
            if (ViewModel.IsSettingsOpen)
                UpdateSettingsOffsetFromPosition();
        }

        /// <summary>
        /// Live-resizes the window from its right/bottom edge and mirrors the new dimensions
        /// into the settings view model. Width maps to <see cref="Window.Width"/>; height maps
        /// to the results-area max height (<see cref="MaxHeight"/> minus the header), matching
        /// how <c>ApplyWindowSize</c> applies the persisted settings.
        /// </summary>
        private void ApplyResize(int dx, int dy)
        {
            double scale = RenderScaling <= 0 ? 1.0 : RenderScaling;

            if (_resizeRight)
            {
                double width = Math.Clamp(_resizeStartWidth + dx / scale, MinWindowWidth, MaxWindowWidth);
                Width = width;
                SettingsView.ViewModel.WindowWidth = (int)width;
            }

            if (_resizeBottom)
            {
                double height = Math.Clamp(_resizeStartResultsHeight + dy / scale, MinResultsHeight, MaxResultsHeight);
                MaxHeight = HeaderSize + height;
                SettingsView.ViewModel.WindowHeight = (int)height;
            }
        }

        /// <summary>
        /// Inverts <see cref="SetWindowPosition"/> to derive the X/Y offset of the current
        /// window position from its anchor on the current screen, writing the result into the
        /// settings view model so a Ctrl+drag updates (and can persist) the placement.
        /// </summary>
        private void UpdateSettingsOffsetFromPosition()
        {
            var screen = Screens.ScreenFromPoint(Position) ?? Screens.Primary;
            if (screen == null) return;

            // Compute the offset relative to the anchor currently selected in the settings
            // dropdown so the saved (anchor + offset) pair reproduces this dragged position.
            var anchor = SettingsView.ViewModel.WindowAnchor;
            var (hFrac, vFrac) = ParseAnchor(anchor);
            var wa = screen.WorkingArea;
            double scaling = screen.Scaling;

            int baseX = wa.X + (int)(hFrac * (wa.Width - Width * scaling));
            int baseY = wa.Y + (int)(vFrac * (wa.Height - HeaderSize * scaling));

            // For bottom anchors the offset is measured from the header-top reference, not
            // the (body-extended) window top.
            int headerTopY = _isBottomAnchor ? _targetHeaderTopY : Position.Y;

            SettingsView.ViewModel.WindowOffsetX = Position.X - baseX;
            SettingsView.ViewModel.WindowOffsetY = headerTopY - baseY;
        }

        private void OnDragPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            // Suppress the click that would otherwise trigger a list/action handler,
            // but only when we actually moved or resized the window.
            if (_dragging) e.Handled = true;
            _dragArmed = false;
            _dragging = false;
            _resizing = false;
            _resizeRight = false;
            _resizeBottom = false;
        }
    }
}
