// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Services.Actions
{
    /// <summary>
    /// Glyphs from the bundled Fluent UI System Icons font (MIT), referenced via the
    /// <c>SymbolFontFamily</c> resource and rendered with <c>FontIcon</c>. Names in the
    /// trailing comments are the source <c>ic_fluent_*_20_regular</c> icons.
    /// </summary>
    internal static class Icons
    {
        // Search window UI
        public const string Close = "\uE6CB";      // dismiss
        public const string Back = "\uE113";       // arrow_left (go back)
        public const string Search = "\uEF9B";     // search
        public const string Save = "\uEF5D";       // save
        public const string Reset = "\uE0C7";      // arrow_counterclockwise_dashes
        public const string Add = "\uE00D";        // add
        public const string Pin = "\uEE3F";        // pin (unpinned)
        public const string PinFilled = "\uEE3E";  // pin (pinned)
        public const string Eye = "\uE87A";        // eye (reveal password)
        public const string EyeOff = "\uE880";     // eye_off (hide password)

        // Action type badges
        public const string Copy = "\uE5D3";       // copy
        public const string Keyboard = "\uEA85";   // keyboard

        // Field icons
        public const string Person = "\uED7D";     // person
        public const string Lock = "\uEB7A";       // lock_closed
        public const string Text = "\uF3E7";       // text_t
        public const string Document = "\uE6E1";   // document
        public const string Globe = "\uE975";      // globe
        public const string Tag = "\uF23D";        // tag
        public const string Clock = "\uE512";      // clock_sparkle (TOTP)

        // Static action icons
        public const string Checkmark = "\uE45C";  // checkmark
        public const string Menu = "\uEC3E";       // more_horizontal
        public const string Settings = "\uEFD3";   // settings

        // Settings section icons
        public const string Options = "\uECC6";    // options (Advanced)
        public const string Info = "\uEA5C";       // info (About)
    }
}
