// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>Configuration for resolving one-time-password (OTP) values.</summary>
    public class OtpOptions
    {
        /// <summary>
        /// Placeholder resolved to produce the time-based OTP value.
        /// Defaults to KeePass's built-in <c>{TIMEOTP}</c>.
        /// </summary>
        public string TotpPlaceholder { get; set; } = Consts.NativeTotpPlaceholder;
    }
}
