using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions
{
    internal class AutoTypeAction : ActionBase
    {
        public override ActionType ActionType => ActionType.AutoType;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database);
        }
    }
}