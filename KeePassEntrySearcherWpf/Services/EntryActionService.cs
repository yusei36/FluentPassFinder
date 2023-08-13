using KeePassEntrySearcherContracts.Services;
using KeePassEntrySearcherWpf.Services.Actions;

namespace KeePassEntrySearcherWpf.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private readonly IEnumerable<IAction> actions;

        public EntryActionService(IEnumerable<IAction> actions)
        {
            this.actions = actions;
        }

        public void RunAction(EntrySearchResult searchResult, ActionType actionType)
        {
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
    }
}
