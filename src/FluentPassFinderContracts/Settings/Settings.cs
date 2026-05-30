// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public
{
    /// <summary>
    /// Root settings object persisted as JSON in KeePass.config.xml.
    /// Every property is initialized to its default, so <c>new Settings()</c>
    /// (or <see cref="CreateDefault"/>) is a fully valid default configuration.
    /// </summary>
    public class Settings
    {
        /// <summary>Schema version, for future migrations of the persisted JSON.</summary>
        public int Version { get; set; } = 1;

        public SearchOptions Search { get; set; } = new SearchOptions();
        public OtpOptions Otp { get; set; } = new OtpOptions();
        public ActionOptions Actions { get; set; } = new ActionOptions();
        public HotkeyOptions Hotkeys { get; set; } = new HotkeyOptions();
        public WindowOptions Window { get; set; } = new WindowOptions();
        public BehaviorOptions Behavior { get; set; } = new BehaviorOptions();
        public AppTheme Theme { get; set; } = AppTheme.System;

        /// <summary>Returns a fresh, fully-defaulted settings instance.</summary>
        public static Settings CreateDefault() => new Settings();
    }
}
