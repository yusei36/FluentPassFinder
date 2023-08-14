using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : ActionBase
    {
        public override ActionType ActionType => ActionType.CopyPassword;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
