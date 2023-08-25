using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private IEnumerable<IStaticAction> staticActions;
        private readonly IPluginProxy pluginProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;

        public EntryActionService(IEnumerable<IStaticAction> actions, IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.pluginProxy = pluginProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;

            InitializeActions(actions, pluginProxy, searchWindowInteractionService);
        }

        private void InitializeActions(IEnumerable<IStaticAction> actions, IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.staticActions = actions.ToList();
            foreach (var action in this.staticActions)
            {
                action.Initialize(pluginProxy, searchWindowInteractionService);
            }
        }

        public void RunAction(EntrySearchResult searchResult, ActionType actionType)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));

            var actions = GetActionsForEntry(searchResult);
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

        public IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult)
        {
            var actions = new List<IAction>();
            actions.AddRange(staticActions);

            foreach (var standardFieldName in PwDefs.GetStandardFields())
            {
                var fieldActions = App.Container.GetAllInstances<IFieldAction>();
                foreach (var fieldAction in fieldActions)
                {
                    fieldAction.Initialize(pluginProxy, searchWindowInteractionService, standardFieldName);
                    actions.Add(fieldAction);
                }
            }
            return actions;
        }
    }
}
