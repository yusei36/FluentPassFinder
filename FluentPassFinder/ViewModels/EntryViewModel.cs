using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using System.Drawing;

namespace FluentPassFinder.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        private readonly IPluginHostProxy hostProxy;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private Image icon;

        public EntrySearchResult SearchResult { get; }

        public EntryViewModel(EntrySearchResult searchResult, IPluginHostProxy hostProxy)
        {
            SearchResult = searchResult;
            this.hostProxy = hostProxy;
            title = searchResult.Entry.Strings.ReadSafe(PwDefs.TitleField);
            userName = searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField);
            url = searchResult.Entry.Strings.ReadSafe(PwDefs.UrlField);
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
                entryIcon = hostProxy.GetBuildInIcon(searchResult.Entry.IconId);
            }
            return entryIcon;
        }
    }
}
