using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.Copy_Password.ToString();

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
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType_Password.ToString();

        public override int SortingIndex => 12;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), searchResult.Entry, searchResult.Database, false);
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, fieldValue + Consts.AutoTypeEnterPlaceholder);
        }
    }
}
