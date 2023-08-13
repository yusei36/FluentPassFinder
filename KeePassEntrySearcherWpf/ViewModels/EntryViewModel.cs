using KeePass.Forms;
using KeePass.UI;
using KeePassEntrySearcherContracts;
using KeePassEntrySearcherContracts.Services;
using KeePassLib;
using System.Diagnostics;
using System.Drawing;

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

        [ObservableProperty]
        private Image? icon;

        public EntryViewModel(EntrySearchResult searchResult, IKeePassInteractionManager interactionManager)
        {
            this.interactionManager = interactionManager;
            entry = searchResult.Entry;
            title = entry.Strings.ReadSafe(PwDefs.TitleField);
            userName = entry.Strings.ReadSafe(PwDefs.UserNameField);
            url = entry.Strings.ReadSafe(PwDefs.UrlField);
            icon = GetEntryIcon(searchResult);
        }

        private Image? GetEntryIcon(EntrySearchResult searchResult)
        {
            Image? entryIcon = null;
            if (!searchResult.Entry.CustomIconUuid.Equals(PwUuid.Zero))
            {
                entryIcon = searchResult.Database.GetCustomIcon(searchResult.Entry.CustomIconUuid, 24, 24);
            }
            if (entryIcon == null)
            {
                entryIcon = interactionManager.GetBuildInIcon(searchResult.Entry.IconId);
            }
            return entryIcon;
        }

        [RelayCommand]
        public void CopyUserName()
        {
            interactionManager.CopyToClipboard(UserName, true, true, entry);
        }

        [RelayCommand]
        public void CopyPassword()
        {
            interactionManager.CopyToClipboard(entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, entry);
        }
    }
}
