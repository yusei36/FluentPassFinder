using FluentPassFinder.Contracts;
using FluentPassFinder.Services.Actions.FieldActions;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.Copy_Password.ToString();

        public override int SortingIndex => 2;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var action = new CopyAction();
            action.Initialize(pluginProxy, searchWindowInteractionService, PwDefs.PasswordField);
            action.RunAction(searchResult);
        }
    }

    internal class AutoTypePasswordAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType_Password.ToString();

        public override int SortingIndex => 12;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var action = new AutoTypeAction();
            action.Initialize(pluginProxy, searchWindowInteractionService, PwDefs.PasswordField);
            action.RunAction(searchResult);
        }
    }
}
