using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder
{
    public class AppProxy : IAppProxy
    {
        public void Init(string pipeName)
        {
            InvokeOnWpfApp(app => app.Init(pipeName));
        }

        public void Shutdown()
        {
            InvokeOnWpfApp(app => app.Shutdown());
            while (App.Current != null)
            {
                Task.Delay(100).Wait();
            }
        }

        public void Main()
        {
            App.Main();
        }

        public void WaitForAppCreation()
        {
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
