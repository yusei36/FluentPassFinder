using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeePassEntrySearcher
{
    internal class MainViewModel : NotifyPropertyChangedBase, IMainViewModel
    {
        private string searchText = string.Empty;

        public string SearchText { 
            get => searchText; 
            set
            { 
                searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SearchResults));
            } 
        }

        public ObservableCollection<ISearchResult> SearchResults 
        { 
            get
            {
                var collection = new ObservableCollection<ISearchResult>();
                foreach (var item in SearchText)
                {
                    collection.Add(new SearchResultViewModel { Text = item.ToString() });
                }
                return collection;
            } 
        }
    }
}
