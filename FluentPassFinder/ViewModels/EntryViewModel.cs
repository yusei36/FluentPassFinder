using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using System.Drawing;

namespace FluentPassFinder.ViewModels
{
    public partial class EntryViewModel : ObservableObject
    {
        private readonly IPluginProxy pluginProxy;
        private const char placeholderStartingChar = '{';

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private Image icon;

        public EntrySearchResult SearchResult { get; }

        public EntryViewModel(EntrySearchResult searchResult, IPluginProxy pluginProxy)
        {
            SearchResult = searchResult;
            this.pluginProxy = pluginProxy;

            var searchOptions = pluginProxy.Settings.SearchOptions;
            title = GetFieldValue(searchResult, PwDefs.TitleField, searchOptions.ResolveFieldReferences);
            userName = GetFieldValue(searchResult, PwDefs.UserNameField, searchOptions.ResolveFieldReferences);
            url = GetFieldValue(searchResult, PwDefs.UrlField, searchOptions.ResolveFieldReferences);
            icon = GetEntryIcon(searchResult);
        }

        private string GetFieldValue(EntrySearchResult searchResult, string fieldName, bool resolveFieldReference)
        {
            var fieldValue = searchResult.Entry.Strings.ReadSafe(fieldName);
            if (resolveFieldReference && fieldValue.Contains(placeholderStartingChar))
            {
                fieldValue = pluginProxy.GetPlaceholderValue(fieldValue, searchResult.Entry, searchResult.Database, false);
            }

            return fieldValue;
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
                entryIcon = pluginProxy.GetBuildInIcon(searchResult.Entry.IconId);
            }
            return entryIcon;
        }
    }
}
