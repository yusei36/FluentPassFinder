using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class AutoTypeAction : ActionBase
    {
        private readonly IPluginHostProxy hostProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;

        public AutoTypeAction(IPluginHostProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }
        public override ActionType ActionType => ActionType.AutoType;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            hostProxy.PerformAutoType(searchResult.Entry, searchResult.Database);
        }
    }
}