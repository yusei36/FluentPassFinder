using FluentPassFinder.Contracts;
using FluentPassFinder.Views;

namespace FluentPassFinder.Services
{
    internal class SearchWindowInteractionService : ISearchWindowInteractionService
    {
        private readonly Lazy<SearchWindow> lazySearchWindow;

        public SearchWindowInteractionService(Lazy<SearchWindow> lazySearchWindow)
        {
            this.lazySearchWindow = lazySearchWindow;
        }
        public void Close()
        {
            lazySearchWindow.Value.HideSearchWindow();
        }
    }
}
