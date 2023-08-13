using FluentPassFinderContracts;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePassLib;
using System;
using System.Drawing;
using System.Windows.Threading;

namespace FluentPassFinderPlugin
{
    internal class PluginHostProxy : IPluginHostProxy
    {
        private Dispatcher pluginHostDispatcher;
        private MainForm mainWindow;

        public PluginHostProxy(IPluginHost pluginHost)
        {
            pluginHostDispatcher = Dispatcher.CurrentDispatcher;
            mainWindow = pluginHost.MainWindow;
        }

        public void CopyToClipboard(string strToCopy, bool bSprCompile, bool bIsEntryInfo, PwEntry peEntryInfo)
        {
            pluginHostDispatcher.Invoke(() =>
            {
                if (ClipboardUtil.Copy(strToCopy, bSprCompile, bIsEntryInfo, peEntryInfo,
                                        mainWindow.DocumentManager.SafeFindContainerOf(peEntryInfo),
                                        IntPtr.Zero))
                {
                    mainWindow.StartClipboardCountdown();
                }
            });
        }

        public Image GetBuildInIcon(PwIcon nuildInIconId)
        {
            return pluginHostDispatcher.Invoke(() => mainWindow.ClientIcons.Images[(int)nuildInIconId]);
        }

        public PwDatabase[] GetPwDatabases()
        {
            return mainWindow?.DocumentManager?.GetOpenDatabases().ToArray();
        }
    }
}
