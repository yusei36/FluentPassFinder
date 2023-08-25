using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

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
            var protectedString = searchResult.Entry.Strings.GetSafe(FieldName);

            // don't read protected strings to prevent having the value in memory
            if (protectedString.IsProtected)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(protectedString.ReadString());
        }
    }
}
