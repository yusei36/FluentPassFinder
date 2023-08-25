using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class AutoTypeEntryAction : ActionBase, IStaticAction
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType.ToString();

        public override int SortingIndex => 0;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database);
        }
    }
}