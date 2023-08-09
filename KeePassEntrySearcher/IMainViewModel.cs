using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KeePassEntrySearcher
{
    internal interface IMainViewModel
    {
        string SearchText { get; set; }

        ObservableCollection<ISearchResult> SearchResults { get; }
    }
}