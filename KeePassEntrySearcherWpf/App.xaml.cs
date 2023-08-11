using KeePassEntrySearcherWpf.Views;
using KeePassEntrySearcherWpf.ViewModels;

namespace KeePassEntrySearcherWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow(new MainWindowViewModel());
        }
    }
}
