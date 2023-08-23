using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        private const string NativeTotpPlacholder = "{TIMEOTP}";

        public override ActionType ActionType => ActionType.Copy_Totp;

        public override int SortingIndex => 3;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypeTotpAction : ActionBase
    {
        private const string NativeTotpPlacholder = "{TIMEOTP}";

        public override ActionType ActionType => ActionType.AutoType_Totp;

        public override int SortingIndex => 13;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, totp);
        }
    }
}
