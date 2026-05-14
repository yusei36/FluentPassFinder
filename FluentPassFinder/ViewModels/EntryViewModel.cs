using Avalonia.Media.Imaging;
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;
using System.IO;

namespace FluentPassFinder.ViewModels
{
    internal partial class EntryViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private Bitmap icon;

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

        public void Dispose() => Icon?.Dispose();

        private static Bitmap LoadIcon(byte[] iconBytes)
        {
            if (iconBytes == null || iconBytes.Length == 0)
                return null;

            using var ms = new MemoryStream(iconBytes);
            return new Bitmap(ms);
        }
    }
}
