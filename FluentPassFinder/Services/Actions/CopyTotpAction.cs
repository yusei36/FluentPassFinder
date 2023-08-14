using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;
using KeePass.Util.Spr;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        private IPluginHostProxy hostProxy;

        public CopyTotpAction(IPluginHostProxy hostProxy)
        {
            this.hostProxy = hostProxy;
        }

        public override ActionType ActionType => ActionType.CopyTotp;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var totp = hostProxy.GetPlaceholderValue("{TIMEOTP}", new SprContext(searchResult.Entry, searchResult.Database, SprCompileFlags.All, true, false));
            if (string.IsNullOrWhiteSpace(totp))
            {
                return;
            }

            hostProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }
}
