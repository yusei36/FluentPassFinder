using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using KeePass.Util.Spr;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        private IPluginHostProxy hostProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;

        public CopyTotpAction(IPluginHostProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        public override ActionType ActionType => ActionType.CopyTotp;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var totp = hostProxy.GetPlaceholderValue("{TIMEOTP}", new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));
            if (string.IsNullOrWhiteSpace(totp))
            {
                hostProxy.CopyToClipboard(string.Empty, true, true, searchResult.Entry);
                return;
            }

            hostProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }
}
