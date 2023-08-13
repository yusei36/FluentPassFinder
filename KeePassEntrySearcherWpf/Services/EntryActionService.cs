using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private readonly IKeePassInteractionManager interactionManager;
        private readonly IKeePassDataProvider keePassDataProvider;

        public EntryActionService(IKeePassInteractionManager interactionManager, IKeePassDataProvider keePassDataProvider)
        {
            this.interactionManager = interactionManager;
            this.keePassDataProvider = keePassDataProvider;
        }


        public void CopyUserName(EntrySearchResult searchResult)
        {
            interactionManager.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }

        public void CopyPassword(EntrySearchResult searchResult)
        {
            interactionManager.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
