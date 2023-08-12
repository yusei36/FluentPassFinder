namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        public EntryViewModel(PwEntry pwEntry)
        {
            title = pwEntry.Strings.ReadSafe(PwDefs.TitleField);
            userName = pwEntry.Strings.ReadSafe(PwDefs.UserNameField);
            url = pwEntry.Strings.ReadSafe(PwDefs.UrlField);
        }
    }
}
