using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services
{
    internal interface IAction
    {
        void RunAction(EntrySearchResult searchResult);

        ActionType ActionType { get; }
    }
}
