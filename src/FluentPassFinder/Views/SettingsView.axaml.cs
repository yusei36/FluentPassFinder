// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Views
{
    internal partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsView() { InitializeComponent(); }

        public SettingsView(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
