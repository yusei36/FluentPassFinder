using KeePassEntrySearcherContracts.Services;

namespace KeePassEntrySearcherWpf.Services.Actions
{
    internal interface IAction
    {
        void RunAction(EntrySearchResult searchResult);

        ActionType ActionType { get; }
    }
}
