using FluentPassFinderContracts;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing;
using System.Windows.Threading;

namespace FluentPassFinderPlugin
{
    internal class PluginProxy : IPluginProxy
    {
        private readonly IPluginHost pluginHost;
        private readonly MainForm mainWindow;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Dispatcher pluginHostDispatcher;
        private readonly Settings settings;

        public PluginProxy(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            pluginHostDispatcher = Dispatcher.CurrentDispatcher;
            mainWindow = pluginHost.MainWindow;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            settings = LoadOrCreateDefaultSettings();
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

        public string GetPlaceholderValue(string placeholder, PwEntry entry, PwDatabase database, bool resolveAll)
        {
            return pluginHostDispatcher.Invoke(() => SprEngine.Compile(placeholder, new SprContext(entry, database, resolveAll ? SprCompileFlags.All : SprCompileFlags.Deref, true, false)));
        }

        public Image GetBuildInIcon(PwIcon buildInIconId)
        {
            return pluginHostDispatcher.Invoke(() => mainWindow.ClientIcons.Images[(int)buildInIconId]);
        }

        public void PerformAutoType(PwEntry entry, PwDatabase database, string sequence = null)
        {
            pluginHostDispatcher.Invoke(() => AutoType.PerformIntoCurrentWindow(entry, database, sequence));
        }

        public PwDatabase[] Databases => mainWindow?.DocumentManager?.GetOpenDatabases().ToArray();

        public Settings Settings => settings;

        private Settings LoadOrCreateDefaultSettings()
        {
            var configString = pluginHost.CustomConfig.GetString(nameof(FluentPassFinderPlugin));
            Settings loadedSettings = null;
            if (configString == null)
            {
                loadedSettings = CreateDefaultSettings();
            }
            else
            {
                try
                {
                    loadedSettings = JsonConvert.DeserializeObject<Settings>(configString, jsonSerializerSettings);
                }
                catch
                {
                    loadedSettings = CreateDefaultSettings();
                }
            }
            return loadedSettings;
        }

        private Settings CreateDefaultSettings()
        {
            var defaultSettings = new Settings()
            {
                SearchOptions = new SearchOptions()
                {
                    IncludeTitleField = true,
                    IncludeNotesField = true,
                    IncludeUrlField = true,
                    IncludeCustomFields = true,
                    IncludeTags = true,
                    ExcludeExpiredEntries = true,
                    ExcludeGroupsBySearchSetting = true,
                    ResolveFieldReferences = true,
                },
                PluginTotpPlaceholder = "{TOTP}",
                GlobalHotkeyCurrentScreen = "Ctrl+Alt+S",
                GlobalHotkeyPrimaryScreen = "Ctrl+Alt+F",
                MainAction = ActionType.OpenContextMenu,
                ShiftAction = ActionType.CopyUserName,
                ControlAction = ActionType.CopyPassword,
                AltAction = ActionType.CopyTotp
            };
            pluginHost.CustomConfig.SetString(nameof(FluentPassFinderPlugin), JsonConvert.SerializeObject(defaultSettings, jsonSerializerSettings));

            return defaultSettings;
        }
    }
}
