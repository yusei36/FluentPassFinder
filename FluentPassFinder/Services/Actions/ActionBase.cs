using FluentPassFinder.Contracts;
using FluentPassFinderContracts;
using System.Runtime;

namespace FluentPassFinder.Services.Actions
{
    internal abstract class ActionBase : IAction
    {
        protected IPluginProxy pluginProxy;
        protected ISearchWindowInteractionService searchWindowInteractionService;

        public abstract void RunAction(EntrySearchResult searchResult);

        public virtual bool CanRunAction(EntrySearchResult searchResult)
        {
            return true;
        }

        public virtual void Initialize(IPluginProxy pluginProxy, ISearchWindowInteractionService searchWindowInteractionService)
        {
            this.pluginProxy = pluginProxy;
            this.searchWindowInteractionService = searchWindowInteractionService;
        }

        protected Settings Settings => pluginProxy.Settings;

        public abstract string ActionType { get; }
        public abstract int DefaultSortingIndex { get; }
        public int SortingIndex
        {
            get
            {
                int configuredSortingIndex;
                if (Settings.ActionSorting != null && Settings.ActionSorting.TryGetValue(ActionType, out configuredSortingIndex))
                {
                    return configuredSortingIndex;
                }
                return DefaultSortingIndex;
            }
        }

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
