// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts;
using FluentPassFinder.Contracts.Public;

namespace FluentPassFinder.Services.Actions.StaticActions
{
    internal class CopyTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 100;
        public override string ActionType => string.Format(Consts.CopyActionPattern, Consts.Totp);
        public override string DisplayName => "Copy 'TOTP'";
        public override string BadgePath => Icons.Copy;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var placeholder = Settings.Otp.TotpPlaceholder;
            if (string.IsNullOrEmpty(placeholder))
                return;

            searchWindowInteractionService.Close();
            pluginProxy.CopyToClipboard(placeholder, searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid);
        }
    }

    internal class AutoTypeTotpAction : TotpActionBase
    {
        public override int DefaultSortingIndex => 150;
        public override string ActionType => string.Format(Consts.AutoTypeActionPattern, Consts.Totp);
        public override string DisplayName => "Auto type 'TOTP'";
        public override string BadgePath => Icons.Keyboard;

        public override void RunAction(EntrySearchResult searchResult)
        {
            var placeholder = Settings.Otp.TotpPlaceholder;
            if (string.IsNullOrEmpty(placeholder))
                return;

            searchWindowInteractionService.CloseThen(() =>
                pluginProxy.PerformAutoType(searchResult.Entry.Uuid, searchResult.Entry.DatabaseUuid, placeholder + Consts.AutoTypeEnterPlaceholder));
        }
    }

    internal abstract class TotpActionBase : ActionBase, IStaticAction
    {
        public override string IconPath => Icons.Clock;

        public override bool CanRunAction(EntrySearchResult searchResult)
        {
            var placeholder = Settings.Otp.TotpPlaceholder;
            if (string.IsNullOrEmpty(placeholder))
                return false;

            return pluginProxy.HasTotp(
                placeholder,
                searchResult.Entry.Uuid,
                searchResult.Entry.DatabaseUuid);
        }
    }
}
