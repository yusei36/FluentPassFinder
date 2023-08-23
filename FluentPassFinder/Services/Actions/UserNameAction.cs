using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : ActionBase
    {
        public override ActionType ActionType => ActionType.Copy_UserName;

        public override int SortingIndex => 1;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), searchResult.Entry, searchResult.Database, false);
            pluginProxy.CopyToClipboard(fieldValue, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypeUserNameAction : ActionBase
    {
        public override ActionType ActionType => ActionType.AutoType_UserName;

        public override int SortingIndex => 11;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), searchResult.Entry, searchResult.Database, false);
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, fieldValue);
        }
    }
}
