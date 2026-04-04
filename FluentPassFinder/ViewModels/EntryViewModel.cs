using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.Drawing;
using System.IO;

namespace FluentPassFinder.ViewModels
{
    internal partial class EntryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private Image icon;

        public EntrySearchResult SearchResult { get; }

        public EntryViewModel(EntrySearchResult searchResult)
        {
            SearchResult = searchResult;

            var entry = searchResult.Entry;
            title    = entry.Title;
            userName = entry.UserName;
            url      = entry.Url;
            icon     = LoadIcon(entry.Icon);
        }

        private static Image LoadIcon(byte[] iconBytes)
        {
            if (iconBytes == null || iconBytes.Length == 0)
                return null;

            // GDI+ keeps a reference to the stream, so clone via Bitmap to detach
            using (var ms = new MemoryStream(iconBytes))
            using (var tmp = new Bitmap(ms))
            {
                return new Bitmap(tmp);
            }
        }
    }
}
