using FluentPassFinder.Contracts;
using FluentPassFinder.Services.Actions.FieldActions;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.Copy_UserName.ToString();

        public override int SortingIndex => 1;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var action = new CopyAction();
            action.Initialize(pluginProxy, searchWindowInteractionService, PwDefs.UserNameField);
            action.RunAction(searchResult);
        }
    }

    internal class AutoTypeUserNameAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType_UserName.ToString();

        public override int SortingIndex => 11;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var action = new AutoTypeAction();
            action.Initialize(pluginProxy, searchWindowInteractionService, PwDefs.UserNameField);
            action.RunAction(searchResult);
        }
    }
}
