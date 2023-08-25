using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class AutoTypeEntryAction : ActionBase, IStaticAction
    {
        public override int DefaultSortingIndex => 0;
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType.ToString();
        public override string DisplayName => "Auto type selected entry";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database);
        }
    }
}