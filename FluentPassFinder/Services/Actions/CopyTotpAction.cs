using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        private const string NativeTotpPlacholder = "{TIMEOTP}";

        public override ActionType ActionType => ActionType.CopyTotp;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = SearchOptions.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database);

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(NativeTotpPlacholder, searchResult.Entry, searchResult.Database);
            }

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }
}
