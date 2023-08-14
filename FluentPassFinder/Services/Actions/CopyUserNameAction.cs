using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : ActionBase
    {
        private readonly IPluginHostProxy hostProxy;

        public override ActionType ActionType => ActionType.CopyUserName;

        public CopyUserNameAction(IPluginHostProxy hostProxy)
        {
            this.hostProxy = hostProxy;
        }

        public override void RunAction(EntrySearchResult searchResult)
        {
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }
    }
}
