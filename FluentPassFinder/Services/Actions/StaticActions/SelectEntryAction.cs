using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class SelectEntryAction : ActionBase, IStaticAction
    {
        public override string DisplayName => "Select entry in main window";

        public override string ActionType => ActionTypeConsts.SelectEntry;

        public override int DefaultSortingIndex => 300;

        public override void RunAction(EntrySearchResult searchResult)
        {
            pluginProxy.SelectEntry(searchResult.Entry, searchResult.Database);
        }
    }
}
