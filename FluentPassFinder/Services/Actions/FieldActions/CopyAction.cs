using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class CopyAction : FieldActionBase
    {
        public override int DefaultSortingIndex => 1000;
        public override string ActionType => string.Format(ActionTypeConsts.CopyActionPattern, FieldName);
        public override string DisplayName => $"Copy '{FieldName}'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(FieldName), searchResult.Entry, searchResult.Database, false);
            pluginProxy.CopyToClipboard(fieldValue, true, true, searchResult.Entry);
        }
    }
}
