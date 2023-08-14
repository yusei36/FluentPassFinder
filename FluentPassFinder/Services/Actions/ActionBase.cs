using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal abstract class ActionBase : IAction
    {
        protected IPluginProxy hostProxy;
        protected ISearchWindowInteractionService searchWindowInteractionService;

        public abstract void RunAction(EntrySearchResult searchResult);

        public virtual bool CanRunAction(EntrySearchResult searchResult)
        {
            return true;
        }

        public virtual void Initialize(IPluginProxy hostProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.hostProxy = hostProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        protected SearchOptions SearchOptions => hostProxy.SearchOptions;

        public abstract ActionType ActionType { get; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var searchResult = parameter as EntrySearchResult;
            if (searchResult != null)
            {
                return CanRunAction(searchResult);
            }
            return false;
        }

        public void Execute(object parameter)
        {
            var searchResult = parameter as EntrySearchResult;
            if (searchResult != null)
            {
                RunAction(searchResult);
            }
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
