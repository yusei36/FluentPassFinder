using FluentPassFinderContracts;

namespace FluentPassFinder
{
    public class AppProxy : IAppProxy
    {
        public void Init(IPluginProxy pluginHostProxy)
        {
            InvokeOnWpfApp((app) => app.Init(pluginHostProxy));
        }
        public void Shutdown()
        {
            InvokeOnWpfApp((app) => app.Shutdown());
            while (App.Current != null)
            {
                Task.Delay(100).Wait();
            }
        }

        public void Main()
        {
            App.Main();
        }

        private void InvokeOnWpfApp(Action<App> action)
        {
            App.Current?.Dispatcher.Invoke(() => action((App)App.Current));
        }

        public void WaitForAppCreation()
        {
            while (App.Current == null)
            {
                Task.Delay(100).Wait();
            }
        }
    }
}
