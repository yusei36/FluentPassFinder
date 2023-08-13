using KeePassLib;

namespace FluentPassFinderContracts.Services
{
    public class EntrySearchResult
    {
        public PwEntry Entry { get; set; }
        public PwDatabase Database { get; set; }
    }
}
