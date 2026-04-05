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
        public static double HeaderSize = 40.0;

        private bool _isClosing;
        private bool _isOpening;

        public SearchWindow(SearchWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            Deactivated += (_, _) => HideSearchWindow();
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void HideSearchWindow()
        {
            if (!_isClosing && !_isOpening)
            {
                _isClosing = true;
                Hide();

                ViewModel.SearchText = string.Empty;
                ViewModel.Entries.Clear();
                ViewModel.IsContextMenuOpen = false;
                ViewModel.SelectedEntry = null;

                _isClosing = false;
            }
        }

        public void ShowSearchWindow(bool showOnPrimaryScreen)
        {
            if (ViewModel.IsAnyDatabaseOpen)
            {
                _isOpening = true;
                SetCenteredWindowPosition(showOnPrimaryScreen);
                Show();
                Activate();
                SearchBox.Focus();
                _isOpening = false;
            }
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem != null)
                lb.ScrollIntoView(lb.SelectedItem);
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
