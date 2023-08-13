namespace KeePassEntrySearcherContracts.Services
{
	public interface IEntryActionService
	{
		void CopyUserName(EntrySearchResult searchResult);
		void CopyPassword(EntrySearchResult searchResult);

		void RunAction(EntrySearchResult searchResult, ActionType actionType);
	}
}
 