using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;
using System.Drawing;

namespace FluentPassFinder.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        private readonly IPluginInteractionManager interactionManager;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private Image? icon;

        public EntrySearchResult SearchResult { get; }

        public EntryViewModel(EntrySearchResult searchResult, IPluginInteractionManager interactionManager)
        {
            SearchResult = searchResult;
            this.interactionManager = interactionManager;
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
                entryIcon = interactionManager.GetBuildInIcon(searchResult.Entry.IconId);
            }
            return entryIcon;
        }
    }
}
