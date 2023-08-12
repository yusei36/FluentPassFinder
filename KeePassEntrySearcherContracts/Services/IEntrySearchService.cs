using KeePassLib;
using System.Collections.Generic;

namespace KeePassEntrySearcherContracts.Services
{
    public interface IEntrySearchService
    {
        IEnumerable<PwEntry> GetPwEntries(IEnumerable<PwDatabase> pwDatabases, string searchQuery);
    }
}
