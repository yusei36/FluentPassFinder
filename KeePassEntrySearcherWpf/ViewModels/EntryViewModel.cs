using CommunityToolkit.Mvvm.ComponentModel;
using KeePassLib;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _url;

        public EntryViewModel(PwEntry pwEntry)
        {
            _title = pwEntry.Strings.ReadSafe(PwDefs.TitleField);
            _userName = pwEntry.Strings.ReadSafe(PwDefs.UserNameField);
            _url = pwEntry.Strings.ReadSafe(PwDefs.UrlField);
        }
    }
}
