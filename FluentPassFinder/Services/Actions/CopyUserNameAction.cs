using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyUserNameAction : ActionBase
    {
        public override ActionType ActionType => ActionType.CopyUserName;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.UserNameField), true, true, searchResult.Entry);
        }
    }
}
