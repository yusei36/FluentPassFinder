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
        }

        public void Main()
        {
            App.Main();

            while (App.Current == null)
            {
                Task.Delay(100).Wait();
            }
        }

        private void InvokeOnWpfApp(Action<App> action)
        {
            App.Current?.Dispatcher.Invoke(() => action((App)App.Current));
        }
    }
}
