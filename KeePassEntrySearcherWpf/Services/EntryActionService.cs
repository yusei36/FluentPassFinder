using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services
{
    internal class EntryActionService : IEntryActionService
    {
        private readonly IKeePassInteractionManager interactionManager;
        private readonly IKeePassDataProvider dataProvider;

        public EntryActionService(IKeePassInteractionManager interactionManager, IKeePassDataProvider dataProvider)
        {
            this.interactionManager = interactionManager;
            this.dataProvider = dataProvider;
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
