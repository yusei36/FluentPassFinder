using KeePass.Plugins;
using KeePassEntrySearcherContracts;
using KeePassEntrySearcherWpf;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassEntrySearcherPlugin
{
    public sealed class KeePassEntrySearcherPluginExt : Plugin, IKeePassDataProvider
    {
        private IPluginHost m_host = null;
        private Thread wpfAppThread;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            m_host = host;

            StartAppAsSeperateThread();

            return true;
        }

        private void HotKeyManager_HotKeyPressed()
        {
            try
            {
                App.Current?.Dispatcher.Invoke(() => App.Current.MainWindow.Show());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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


            App.Current?.Dispatcher.Invoke(() => ((App)App.Current).Init(this));
        }

        private void StartAppInternal()
        {
            App.Main();
        }
    }
}
