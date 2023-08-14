using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private readonly IEnumerable<IAction> actions;

        public EntryActionService(IEnumerable<IAction> actions)
        {
            this.actions = actions;
        }

        public IEnumerable<IAction> Actions => actions;

        public void RunAction(EntrySearchResult searchResult, ActionType actionType)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));

            var action = actions.FirstOrDefault(a => a.ActionType == actionType);
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
