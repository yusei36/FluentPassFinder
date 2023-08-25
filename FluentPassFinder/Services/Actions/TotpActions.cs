using FluentPassFinder.Contracts;
using FluentPassFinderContracts;

namespace FluentPassFinder.Services.Actions
{
    internal class CopyTotpAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.Copy_Totp.ToString();

        public override int SortingIndex => 3;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(Consts.NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.CopyToClipboard(totp, true, true, searchResult.Entry);
        }
    }

    internal class AutoTypeTotpAction : ActionBase
    {
        public override string ActionType => FluentPassFinderContracts.ActionType.AutoType_Totp.ToString();

        public override int SortingIndex => 13;

        public override void RunAction(EntrySearchResult searchResult)
        {
            searchWindowInteractionService.Close();
            var pluginTotpPlaceholder = Settings.PluginTotpPlaceholder;
            var totp = pluginProxy.GetPlaceholderValue(pluginTotpPlaceholder, searchResult.Entry, searchResult.Database, true);

            if (String.IsNullOrEmpty(totp) || totp == pluginTotpPlaceholder)
            {
                totp = pluginProxy.GetPlaceholderValue(Consts.NativeTotpPlacholder, searchResult.Entry, searchResult.Database, true);
            }

            pluginProxy.PerformAutoType(searchResult.Entry, searchResult.Database, totp + Consts.AutoTypeEnterPlaceholder);
        }
    }
}
