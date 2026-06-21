// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Threading;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Views
{
    internal partial class CreateEntryView : UserControl
    {
        public CreateEntryViewModel ViewModel { get; }

        public CreateEntryView() { InitializeComponent(); }

        public CreateEntryView(CreateEntryViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        /// <summary>Focuses the Title field. Called by the host window when the overlay opens.</summary>
        public void FocusFirstField() =>
            Dispatcher.UIThread.Post(() => TitleBox.Focus(), DispatcherPriority.Background);
    }
}
