using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : ActionBase
    {
        private readonly IPluginHostProxy hostProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;

        public override ActionType ActionType => ActionType.CopyUserName;

        public CopyUserNameAction(IPluginHostProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }
    }
}
