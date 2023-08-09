using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassEntrySearcher
{
    internal class SearchResultViewModel : NotifyPropertyChangedBase, ISearchResult
    {
        private string text = string.Empty;

        public string Text 
        { 
            get => text; 
            set 
            { 
                text = value;
                OnPropertyChanged();
            }
        }
    }
}
