﻿using FluentPassFinderContracts;
using FluentPassFinderContracts.Services;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyPasswordAction : IAction
    {
        private readonly IPluginHostProxy hostProxy;

        public ActionType ActionType => ActionType.CopyPassword;

        public CopyPasswordAction(IPluginHostProxy hostProxy)
        {
            this.hostProxy = hostProxy;
        }

        public void RunAction(EntrySearchResult searchResult)
        {
            hostProxy.CopyToClipboard(searchResult.Entry.Strings.ReadSafe(PwDefs.PasswordField), true, true, searchResult.Entry);
        }
    }
}
