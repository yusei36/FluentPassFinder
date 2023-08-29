using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class CopyTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 100;
        public override string ActionType => string.Format(ActionTypeConsts.CopyActionPattern, ActionTypeConsts.Totp);
        public override string DisplayName => "Copy 'TOTP'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();

            var totp = GenerateTotp(searchResult);

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypeTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 150;
        public override string ActionType => string.Format(ActionTypeConsts.AutoTypeActionPattern, ActionTypeConsts.Totp);
        public override string DisplayName => "Auto type 'TOTP'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            Task.Delay(100).Wait();

            var totp = GenerateTotp(searchResult);

            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, totp + Consts.AutoTypeEnterPlaceholder);
        }
    }

    internal abstract class TotpActionBase : ActionBase, IStaticAction
    {
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
                totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);
            }

            if ((string.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder) && CanGenerateNativeTotp(searchResult))
            {
                totp = pluginProxy.GetPlaceholderValue(Consts.NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            return totp;
        }

        private bool CanGeneratePluginTotp(EntrySearchResult searchResult)
        {
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            if (pluginTotpPlaceholder != null)
            {
                var pluginTotpFieldConfig = Settings.PluginTotpFieldConfig;
                if (pluginTotpFieldConfig == null)
                {
                    return true;
                }
                else
                {
                    var configuredField = pluginProxy.GetStringFromCustomConfig(pluginTotpFieldConfig, null);
                    if (!string.IsNullOrEmpty(configuredField) && searchResult.Entry.Strings.GetKeys().Contains(configuredField))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CanGenerateNativeTotp(EntrySearchResult searchResult)
        {
            return searchResult.Entry.Strings.GetKeys().Any(x => x.StartsWith(Consts.NativeTotpFieldPrefix, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
