using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        private readonly IPluginHostProxy hostProxy;

        public override ActionType ActionType => ActionType.CopyPassword;

        public CopyPasswordAction(IPluginHostProxy hostProxy)
        {
            this.hostProxy = hostProxy;
        }

        public override void RunAction(EntrySearchResult searchResult)
        {
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
