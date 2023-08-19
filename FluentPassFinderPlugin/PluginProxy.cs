using FluentPassFinderContracts;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using System;
using System.Drawing;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace FluentPassFinderPlugin
{
    internal class PluginProxy : IPluginProxy
    {
        private readonly IPluginHost pluginHost;
        private Dispatcher pluginHostDispatcher;
        private MainForm mainWindow;
        private Settings searchOptions;

        public PluginProxy(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            pluginHostDispatcher = Dispatcher.CurrentDispatcher;
            mainWindow = pluginHost.MainWindow;
            LoadOrCreateDefaultSettings();
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

        public string GetPlaceholderValue(string placeholder, PwEntry entry, PwDatabase database)
        {
            return pluginHostDispatcher.Invoke(() => SprEngine.Compile(placeholder, new SprContext(entry, database, SprCompileFlags.All, true, false)));
        }

        public Image GetBuildInIcon(PwIcon buildInIconId)
        {
            return pluginHostDispatcher.Invoke(() => mainWindow.ClientIcons.Images[(int)buildInIconId]);
        }

        public PwDatabase[] Databases => mainWindow?.DocumentManager?.GetOpenDatabases().ToArray();

        public Settings Settings
        {
            get
            {
                return searchOptions;
            }
        }

        private void LoadOrCreateDefaultSettings()
        {
            var configString = pluginHost.CustomConfig.GetString(nameof(FluentPassFinderPlugin));
            
            if (configString == null)
            {
                var defaultSettings = new Settings()
                {
                    SearchOptions = new SearchOptions()
                    {
                        IncludeTitleFiled = true,
                        IncludeNotesField = true,
                        IncludeUrlField = true,
                        IncludeCustomFields = true,
                    },
                    PluginTotpPlaceholder = "{TOTP}",
                    GlobalHotkeyCurrentScreen = "Ctrl+Alt+S",
                    GlobalHotkeyPrimaryScreen = "Ctrl+Alt+F"
                };
                
                pluginHost.CustomConfig.SetString(nameof(FluentPassFinderPlugin), JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
                searchOptions = defaultSettings;
            }
            else
            {
                searchOptions = JsonConvert.DeserializeObject<Settings>(configString);
            }
        }

        public void PerformAutoType(PwEntry entry, PwDatabase database, string sequence = null)
        {
            pluginHostDispatcher.Invoke(() => AutoType.PerformIntoCurrentWindow(entry, database, sequence));
        }
    }
}
