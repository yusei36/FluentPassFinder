using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services.Actions
{
    internal class CopyPasswordAction : IAction
    {
        private readonly IKeePassInteractionManager interactionManager;

        public ActionType ActionType => ActionType.CopyPassword;

        public CopyPasswordAction(IKeePassInteractionManager interactionManager)
        {
            this.interactionManager = interactionManager;
        }

        public void RunAction(EntrySearchResult searchResult)
        {
            interactionManager.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
