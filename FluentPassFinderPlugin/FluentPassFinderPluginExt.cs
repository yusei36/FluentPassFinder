using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using FluentPassFinderContracts;
using FluentPassFinder;
using KeePassLib;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FluentPassFinderPlugin
{
    public sealed class FluentPassFinderPluginExt : Plugin, IPluginDataProvider, IPluginInteractionManager
    {
        private IPluginHost pluginHost = null;
        private Thread wpfAppThread;
        private Dispatcher winFormsDispatcher;


        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            pluginHost = host;
            winFormsDispatcher = Dispatcher.CurrentDispatcher;

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
            wpfAppThread = new Thread(StartAppInternal);
            wpfAppThread.SetApartmentState(ApartmentState.STA);
            wpfAppThread.Start();

            // wait until app is initialized
            while (App.Current == null)
            {
                Task.Delay(100).Wait();
            }

            InvokeOnWpfApp((app) => app.Init(this, this));
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
            return pluginHost?.MainWindow?.DocumentManager?.GetOpenDatabases().ToArray();
        }

        public void StartClipboardCountdown()
        {
            pluginHost?.MainWindow?.StartClipboardCountdown();
        }

        public void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo)
        {
            PluginHostDispatcher.Invoke(() =>
            {
                if (ClipboardUtil.Copy(strToCopy, bSprCompile, bIsEntryInfo, peEntryInfo,
                                        MainForm.DocumentManager.SafeFindContainerOf(peEntryInfo),
                                        IntPtr.Zero))
                {
                    MainForm.StartClipboardCountdown();
                }
            });
        }

        public Image GetBuildInIcon(PwIcon nuildInIconId)
        {
            return PluginHostDispatcher.Invoke(() => MainForm.ClientIcons.Images[(int)nuildInIconId]);
        }

        public MainForm MainForm => pluginHost?.MainWindow;
        public Dispatcher PluginHostDispatcher => winFormsDispatcher;
    }
}
