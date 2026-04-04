using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.Threading.Tasks;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class AutoTypeAction : FieldActionBase
    {
        public override int DefaultSortingIndex => 2000;
        public override string ActionType => string.Format(ActionTypeConsts.AutoTypeActionPattern, FieldName);
        public override string DisplayName => $"Auto type '{FieldName}'";

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            Task.Delay(100).Wait();
            pluginProxy.AutoTypeField(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid, FieldName);
        }
    }
}
