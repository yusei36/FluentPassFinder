using KeePassEntrySearcherContracts;

namespace KeePassEntrySearcherWpf.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        private PwEntry entry;
        private readonly IKeePassInteractionManager interactionManager;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        public EntryViewModel(PwEntry entry, IKeePassInteractionManager interactionManager)
        {
            this.entry = entry;
            this.interactionManager = interactionManager;
            title = entry.Strings.ReadSafe(PwDefs.TitleField);
            userName = entry.Strings.ReadSafe(PwDefs.UserNameField);
            url = entry.Strings.ReadSafe(PwDefs.UrlField);
        }

        [RelayCommand]
        private void OnCopyUserName()
        {
            interactionManager.CopyToClipboard(UserName, true, true, entry);
        }
    }
}
