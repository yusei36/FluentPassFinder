namespace KeePassEntrySearcherContracts.Services
{
	public interface IEntryActionService
	{
		void RunAction(EntrySearchResult searchResult, ActionType actionType);
	}
}
 