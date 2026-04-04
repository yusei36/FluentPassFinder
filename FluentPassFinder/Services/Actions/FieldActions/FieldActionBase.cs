using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal abstract class FieldActionBase : ActionBase, IFieldAction
    {
        public string FieldName { get; private set; }

        public void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService, string fieldName)
        {
            FieldName = fieldName;
            Initialize(hostProxy, searchWindowInteractionService);
        }

        public override bool CanRunAction(EntrySearchResult searchResult)
        {
            if (!searchResult.Entry.Fields.TryGetValue(FieldName, out var field))
                return false;

            return field.HasValue;
        }
    }
}
