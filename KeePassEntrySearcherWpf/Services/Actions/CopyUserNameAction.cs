using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services.Actions
{
    internal class CopyUserNameAction : IAction
    {
        private readonly IPluginInteractionManager interactionManager;

        public ActionType ActionType => ActionType.CopyUserName;

        public CopyUserNameAction(IPluginInteractionManager interactionManager)
        {
            this.interactionManager = interactionManager;
        }

        public void RunAction(EntrySearchResult searchResult)
        {
            interactionManager.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }
    }
}
