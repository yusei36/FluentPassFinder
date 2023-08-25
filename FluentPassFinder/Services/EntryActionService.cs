using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using KeePassLib;
using System.Runtime;

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

            var actions = GetActionsForEntry(searchResult, true);
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

        public IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult, bool inculdeHiddenActions)
        {
            var actions = new List<IAction>();
            actions.AddRange(staticActions);

            var fields = GetFields(searchResult, inculdeHiddenActions);
            foreach (var standardFieldName in fields)
            {
                var fieldActions = App.Container.GetAllInstances<IFieldAction>();
                foreach (var fieldAction in fieldActions)
                {
                    fieldAction.Initialize(pluginProxy, searchWindowInteractionService, standardFieldName);
                    actions.Add(fieldAction);
                }
            }

            if (!inculdeHiddenActions)
            {
                actions = actions.Where(a => a.SortingIndex >= 0 && a.CanExecute(searchResult)).ToList();
            }

            return actions.OrderBy(a => a.SortingIndex);
        }

        private IEnumerable<string> GetFields(EntrySearchResult searchResult, bool inculdeHiddenActions)
        {
            var settings = pluginProxy.Settings;
            var fields = new List<string>();
            fields.AddRange(PwDefs.GetStandardFields());

            if (inculdeHiddenActions || (!inculdeHiddenActions && settings.ShowActionsForCustomFields))
            {
                var customFields = searchResult.Entry.Strings.GetKeys().Where(fieldName => !PwDefs.IsStandardField(fieldName));
                fields.AddRange(customFields);
            }

            if (!inculdeHiddenActions)
            {
                return fields.Where(f => !settings.ExcludeActionsForFields.Any(excludeField => f.Equals(excludeField)));
            }

            return fields;
        }
    }
}
