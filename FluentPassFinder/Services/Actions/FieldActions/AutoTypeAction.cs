using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class AutoTypeAction : FieldActionBase
    {
        public override int DefaultSortingIndex => 2000;
        public override string ActionType => $"AutoType_{FieldName}";
        public override string DisplayName => $"Auto type '{FieldName}'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(FieldName), searchResult.Entry, searchResult.Database, false);
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, fieldValue + Consts.AutoTypeEnterPlaceholder);
        }
    }
}
