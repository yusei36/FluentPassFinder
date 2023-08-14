using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions
{
    internal abstract class ActionBase : IAction
    {
        public abstract ActionType ActionType { get; }

        public event EventHandler? CanExecuteChanged;

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

        public abstract void RunAction(EntrySearchResult searchResult);

        public virtual bool CanRunAction(EntrySearchResult searchResult)
        {
            return true;
        }
        
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
