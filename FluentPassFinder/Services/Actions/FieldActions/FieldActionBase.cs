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
    }
}
