using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using KeePass.Util.Spr;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        private IPluginHostProxy hostProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;
        private const string NativeTotpPlacholder = "{TIMEOTP}";
        public CopyTotpAction(IPluginHostProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        public override ActionType ActionType => ActionType.CopyTotp;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = hostProxy.SearchOptions.PluginTotpPlaceholder;
            var totp = hostProxy.GetPlaceholderValue(pluginTotpPlaceholder, new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = hostProxy.GetPlaceholderValue(NativeTotpPlacholder, new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));
            }

            hostProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }
}
