﻿using FluentPassFinder.ViewModels;
using System.Windows.Controls;

namespace FluentPassFinder.Views
{
    public partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }
        private bool isClosing = false;
        private bool isOpening = false;

        public SearchWindow(SearchWindowViewModel viewModel)
        {
            Wpf.Ui.Appearance.Watcher.Watch(this);

            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            HideSearchWindow();
        }

        [RelayCommand]
        public void HideSearchWindow()
        {
            if (!isClosing && !isOpening)
            {
                isClosing = true;
                Hide();

                ViewModel.SearchText = string.Empty;
                ViewModel.Entries.Clear();

                ViewModel.IsContextMenuOpen = false;
                ViewModel.SelectedEntry = null;
                ViewModel.SelectedContextAction = ViewModel.ContextActions.First();

                isClosing = false;
            }
        }

        public void ShowSearchWindow()
        {
            if (ViewModel.IsAnyDatabaseOpen)
            {
                isOpening = true;
                Show();
                Activate();
                SearchBox.Focus();
                isOpening = false;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }
    }
}
