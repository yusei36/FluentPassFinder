﻿using FluentPassFinder.Contracts;

namespace FluentPassFinder.Services.Actions.FieldActions
{
    internal class AutoTypeActions : FieldActionBase
    {
        public override string ActionType => "AutoType_"+FieldName;

        public override int SortingIndex => 200;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var fieldValue = pluginProxy.GetPlaceholderValue(searchResult.Entry.Strings.ReadSafe(FieldName), searchResult.Entry, searchResult.Database, false);
            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, fieldValue + Consts.AutoTypeEnterPlaceholder);
        }
    }
}
