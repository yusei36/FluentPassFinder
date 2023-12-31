﻿using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class OpenUrlAction : ActionBase, IStaticAction
    {
        public override string DisplayName => "Open URL of selected entry";

        public override string ActionType => ActionTypeConsts.OpenUrl;

        public override int DefaultSortingIndex => 200;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            pluginProxy.OpenEntryUrl(searchResult.Entry);
        }
    }
}
