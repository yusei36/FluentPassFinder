using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using KeePass.Util.Spr;

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
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(NativeTotpPlacholder, new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));
            }

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }
}
