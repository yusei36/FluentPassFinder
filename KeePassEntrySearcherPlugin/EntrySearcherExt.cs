using KeePass.Plugins;
using KeePassEntrySearcherWpf;
using KeePassEntrySearcherWpf.Services;
using KeePassEntrySearcherWpf.Views.Windows;
using System;
using System.Windows.Forms;

namespace EntrySearcher
{
    public sealed class EntrySearcherExt : Plugin
    {
        private IPluginHost m_host = null;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            m_host = host;
            HotKeyManager.RegisterHotKey(Keys.Control | Keys.Alt | Keys.D);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            return true;
        }

        private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            try
            {
                var mainWindow = new MainWindow(new KeePassEntrySearcherWpf.ViewModels.Windows.MainWindowViewModel());
                mainWindow.Show();
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
