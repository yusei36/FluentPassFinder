// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class CopyTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 100;
        public override string ActionType => string.Format(Consts.CopyActionPattern, Consts.Totp);
        public override string DisplayName => "Copy 'TOTP'";
        public override string BadgePath => Icons.Copy;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var totp = GenerateTotp(searchResult);
            if (!string.IsNullOrEmpty(totp))
                pluginProxy.CopyToClipboard(totp, searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid);
        }
    }

    internal class AutoTypeTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 150;
        public override string ActionType => string.Format(Consts.AutoTypeActionPattern, Consts.Totp);
        public override string DisplayName => "Auto type 'TOTP'";
        public override string BadgePath => Icons.Keyboard;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            Task.Delay(100).Wait();
            var totp = GenerateTotp(searchResult);
            if (!string.IsNullOrEmpty(totp))
                pluginProxy.PerformAutoType(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid, totp + Consts.AutoTypeEnterPlaceholder);
        }
    }

    internal abstract class TotpActionBase : ActionBase, IStaticAction
    {
        public override string IconPath => Icons.Clock;

        public override bool CanRunAction(EntrySearchResult searchResult)
        {
            return CanGeneratePluginTotp(searchResult) || CanGenerateNativeTotp(searchResult);
        }

        protected string GenerateTotp(EntrySearchResult searchResult)
        {
            string totp = null;
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;

            if (CanGeneratePluginTotp(searchResult))
            {
                totp = pluginProxy.GetPlaceholderValue(
                    pluginTotpPlaceholder,
                    searchResult.Entry.Uuid,
                    searchResult.Entry.DatabaseUuid,
                    resolveAll: true);
            }

            if ((string.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder) && CanGenerateNativeTotp(searchResult))
            {
                totp = pluginProxy.GetPlaceholderValue(
                    Consts.NativeTotpPlacholder,
                    searchResult.Entry.Uuid,
                    searchResult.Entry.DatabaseUuid,
                    resolveAll: true);
            }

            return totp;
        }

        private bool CanGeneratePluginTotp(EntrySearchResult searchResult)
        {
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            if (pluginTotpPlaceholder == null)
                return false;

            var pluginTotpFieldConfig = Settings.PluginTotpFieldConfig;
            if (pluginTotpFieldConfig == null)
                return true;

            var configuredField = pluginProxy.GetStringFromCustomConfig(pluginTotpFieldConfig, null);
            return !string.IsNullOrEmpty(configuredField)
                   && searchResult.Entry.Fields.ContainsKey(configuredField);
        }

        private static bool CanGenerateNativeTotp(EntrySearchResult searchResult)
        {
            return searchResult.Entry.Fields.Keys
                .Any(k => k.StartsWith(Consts.NativeTotpFieldPrefix, System.StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
