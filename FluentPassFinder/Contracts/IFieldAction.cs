using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Contracts
{
    internal interface IFieldAction : IAction
    {
        void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService, string fieldName);
    }
}
