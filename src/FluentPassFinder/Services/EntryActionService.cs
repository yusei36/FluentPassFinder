// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPassFinder.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private IEnumerable<IStaticAction> staticActions;
        private readonly IPluginProxy pluginProxy;
        private readonly ISearchWindowInteractionService searchWindowInteractionService;
        private readonly IServiceProvider serviceProvider;

        public EntryActionService(IEnumerable<IStaticAction> actions, IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService, IServiceProvider serviceProvider)
        {
            this.pluginProxy = pluginProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
            this.serviceProvider = serviceProvider;
            InitializeActions(actions);
        }

        private void InitializeActions(IEnumerable<IStaticAction> actions)
        {
            this.staticActions = actions.ToList();
            foreach (var action in this.staticActions)
                action.Initialize(pluginProxy, searchWindowInteractionService);
        }

        public void RunAction(EntrySearchResult searchResult, string actionType)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));

            var action = GetActionsForEntry(searchResult, true).FirstOrDefault(a => a.ActionType == actionType);
            if (action != null)
                action.RunAction(searchResult);
            else
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType.ToString());
        }

        public void RunAction(EntrySearchResult searchResult, IAction action)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            if (action == null) throw new ArgumentNullException(nameof(action));
            action.RunAction(searchResult);
        }

        public IEnumerable<IAction> GetActionsForEntry(EntrySearchResult searchResult, bool includeHiddenActions)
        {
            var actions = new List<IAction>();
            actions.AddRange(staticActions);

            foreach (var fieldName in GetFields(searchResult, includeHiddenActions))
            {
                var fieldActions = serviceProvider.GetServices<IFieldAction>();
                foreach (var fieldAction in fieldActions)
                {
                    fieldAction.Initialize(pluginProxy, searchWindowInteractionService, fieldName);
                    actions.Add(fieldAction);
                }
            }

            if (!includeHiddenActions)
                actions = actions.Where(a => a.SortingIndex >= 0 && a.CanExecute(searchResult)).ToList();

            return actions.OrderBy(a => a.SortingIndex);
        }

        private IEnumerable<string> GetFields(EntrySearchResult searchResult, bool includeHiddenActions)
        {
            var settings = pluginProxy.Settings;
            var fields = new List<string>(Consts.StandardFieldNames);

            if (includeHiddenActions || settings.Actions.ShowForCustomFields)
            {
                var customFields = searchResult.Entry.Fields.Keys
                    .Where(fieldName => !Consts.IsStandardField(fieldName));
                fields.AddRange(customFields);
            }

            if (!includeHiddenActions)
                return fields.Where(f => !settings.Actions.ExcludeForFields.Any(ex => f.Equals(ex)));

            return fields;
        }
    }
}
