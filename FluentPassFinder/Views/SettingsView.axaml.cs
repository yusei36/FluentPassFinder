using Avalonia.Controls;
using FluentPassFinder.ViewModels;

namespace FluentPassFinder.Views
{
    internal partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsView(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
