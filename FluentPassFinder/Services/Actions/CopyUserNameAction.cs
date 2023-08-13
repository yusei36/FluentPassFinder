using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : IAction
    {
        private readonly IPluginHostProxy hostProxy;

        public ActionType ActionType => ActionType.CopyUserName;

        public CopyUserNameAction(IPluginHostProxy hostProxy)
        {
            this.hostProxy = hostProxy;
        }

        public void RunAction(EntrySearchResult searchResult)
        {
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }
    }
}
