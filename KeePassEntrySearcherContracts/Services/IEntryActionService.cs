using KeePassLib;
using System.Collections.Generic;

namespace KeePassEntrySearcherContracts.Services
{
	public interface IEntryActionService
	{
		void CopyUserName(EntrySearchResult searchResult);
		void CopyPassword(EntrySearchResult searchResult);
	}
}
 