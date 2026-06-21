// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public;
using KeePass.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Owns the plugin's <see cref="Settings"/> and persists them as JSON in KeePass's custom
    /// config (<c>KeePass.config.xml</c>). The single source of truth other services read from.
    /// </summary>
    internal class SettingsStore
    {
        private const string ConfigKey = "FluentPassFinder";

        private readonly IPluginHost pluginHost;
        private readonly KeePassContext context;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public Settings Current { get; private set; }

        public SettingsStore(IPluginHost pluginHost, KeePassContext context)
        {
            this.pluginHost = pluginHost;
            this.context = context;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            // Replace default-initialized collections instead of appending to them,
            // otherwise default entries (e.g. ExcludeFields) duplicate on every load/save round-trip.
            jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            Current = LoadOrCreateDefault();
        }

        public void Save(Settings settings)
        {
            Current = settings;
            context.Invoke(() =>
                pluginHost.CustomConfig.SetString(ConfigKey,
                    JsonConvert.SerializeObject(Current, jsonSerializerSettings)));
        }

        private Settings LoadOrCreateDefault()
        {
            var configString = pluginHost.CustomConfig.GetString(ConfigKey);
            if (configString == null)
                return CreateDefault();

            try
            {
                return JsonConvert.DeserializeObject<Settings>(configString, jsonSerializerSettings);
            }
            catch
            {
                return CreateDefault();
            }
        }

        private Settings CreateDefault()
        {
            var defaults = Settings.CreateDefault();
            pluginHost.CustomConfig.SetString(ConfigKey,
                JsonConvert.SerializeObject(defaults, jsonSerializerSettings));
            return defaults;
        }
    }
}
