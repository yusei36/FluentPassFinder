using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        public override ActionType ActionType => ActionType.Copy_Password;

        public override int SortingIndex => 2;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), searchResult.Entry, searchResult.Database, false);
            pluginProxy.CopyToClipboard(fieldValue, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypePasswordAction : ActionBase
    {
        public override ActionType ActionType => ActionType.AutoType_Password;

        public override int SortingIndex => 12;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), searchResult.Entry, searchResult.Database, false);
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, fieldValue);
        }
    }
}
