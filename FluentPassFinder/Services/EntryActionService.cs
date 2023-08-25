using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private IEnumerable<IAction> actions;

        public EntryActionService(IEnumerable<IAction> actions, IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            InitializeActions(actions, pluginProxy, searchWindowInteractionService);
        }

        private void InitializeActions(IEnumerable<IAction> actions, IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.actions = actions.Where(x => x is not IFieldAction).ToList();
            foreach (var action in this.actions)
            {
                action.Initialize(pluginProxy, searchWindowInteractionService);
            }
        }

        public IEnumerable<IAction> Actions => actions;

        public void RunAction(EntrySearchResult searchResult, ActionType actionType)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));

            var action = actions.FirstOrDefault(a => a.ActionType == actionType.ToString());
            if (action != null)
            {
                action.RunAction(searchResult);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType.ToString());
            }
        }

        public void RunAction(EntrySearchResult searchResult, IAction action)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            if (action == null) throw new ArgumentNullException(nameof(action));

            action.RunAction(searchResult);
        }
    }
}
