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

        public abstract string DisplayName { get; }
        public abstract string ActionType { get; }
        public abstract int DefaultSortingIndex { get; }
        public int SortingIndex
        {
            get
            {
                if (Settings.ActionSorting != null && Settings.ActionSorting.TryGetValue(ActionType, out int configuredSortingIndex))
                {
                    return configuredSortingIndex;
                }
                return DefaultSortingIndex;
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is EntrySearchResult searchResult)
            {
                return CanRunAction(searchResult);
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is EntrySearchResult searchResult)
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
