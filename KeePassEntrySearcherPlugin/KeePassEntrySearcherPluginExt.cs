using KeePass.Forms;
using KeePass.Plugins;
using KeePassEntrySearcherContracts;
using KeePassEntrySearcherWpf;
using KeePassLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassEntrySearcherPlugin
{
    public sealed class KeePassEntrySearcherPluginExt : Plugin, IKeePassDataProvider
    {
        private IPluginHost _host = null;
        private Thread wpfAppThread;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            _host = host;

            StartAppAsSeperateThread();

            _host.MainWindow.DocumentManager.GetOpenDatabases();

            return true;
        }

        public override void Terminate()
        {
            wpfAppThread?.Abort();
            base.Terminate();
        }

        private void StartAppAsSeperateThread()
        {
            wpfAppThread = new Thread(StartAppInternal);
            wpfAppThread.SetApartmentState(ApartmentState.STA);
            wpfAppThread.Start();

            // wait until app is initialized
            while (App.Current == null)
            {
                Task.Delay(100).Wait();
            }

            InvokeOnWpfApp((app) => app.Init(this));
        }

        private void InvokeOnWpfApp(Action<App> action)
        {
            App.Current?.Dispatcher.Invoke(() => action((App)App.Current));
        }

        private void StartAppInternal()
        {
            App.Main();
        }

        public PwDatabase[] GetPwDatabases()
        {
            return _host?.MainWindow?.DocumentManager?.GetOpenDatabases().ToArray();
        }
    }
}
