using FluentPassFinder;
using FluentPassFinderContracts;
using KeePass.Plugins;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentPassFinderPlugin
{
    public sealed class FluentPassFinderPluginExt : Plugin
    {
        private IPluginHost pluginHost = null;
        private Thread wpfAppThread;
        private IPluginHostProxy pluginHostProxy;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            pluginHost = host;
            pluginHostProxy = new PluginHostProxy(pluginHost);

            StartAppAsSeperateThread();

            pluginHost.MainWindow.DocumentManager.GetOpenDatabases();

            return true;
        }

        public override void Terminate()
        {
            wpfAppThread?.Abort();
            base.Terminate();
        }

        private void StartAppAsSeperateThread()
        {
            wpfAppThread = new Thread(App.Main);
            wpfAppThread.SetApartmentState(ApartmentState.STA);
            wpfAppThread.Start();

            // wait until app is initialized
            while (App.Current == null)
            {
                Task.Delay(100).Wait();
            }

            InvokeOnWpfApp((app) => app.Init(pluginHostProxy));
        }

        private void InvokeOnWpfApp(Action<App> action)
        {
            App.Current?.Dispatcher.Invoke(() => action((App)App.Current));
        }
    }
}
