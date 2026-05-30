// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Configuration for resolving TOTP values via the KeePass TOTP plugin.</summary>
    public class TotpOptions
    {
        /// <summary>Placeholder the TOTP plugin exposes (e.g. <c>{TOTP}</c>).</summary>
        public string Placeholder { get; set; } = Consts.PluginTotpPlaceholder;

        /// <summary>Key (in KeePass.config.xml) of the field name the TOTP plugin reads from.</summary>
        public string FieldConfigKey { get; set; } = Consts.PluginTotpFieldConfigKey;
    }
}
