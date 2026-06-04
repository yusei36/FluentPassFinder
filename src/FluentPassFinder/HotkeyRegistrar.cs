// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;
using SharpHook;
using SharpHook.Data;

namespace FluentPassFinder
{
    /// <summary>
    /// Registers global hotkeys via SharpHook's low-level global keyboard hook.
    /// SharpHook sees every keystroke; we match registered gestures against the
    /// pressed key plus modifier mask and suppress the event so the combination is
    /// not delivered to the focused application (mirroring RegisterHotKey behaviour).
    /// </summary>
    internal static class HotkeyRegistrar
    {
        [Flags]
        private enum Mods
        {
            None = 0,
            Ctrl = 1,
            Alt = 2,
            Shift = 4,
            Win = 8,
        }

        private sealed record Hotkey(Mods Modifiers, KeyCode Key, Action Callback);

        private static readonly Dictionary<string, Hotkey> _byName = new();
        private static readonly object _lock = new();

        private static IGlobalHook _hook;

        /// <summary>
        /// Register a global hotkey. Gesture format: "Ctrl+Alt+K", "Shift+Win+F1", etc.
        /// Safe to call multiple times from any thread.
        /// </summary>
        public static void Register(string name, string gestureString, Action callback)
        {
            if (string.IsNullOrWhiteSpace(gestureString) || callback == null)
                return;

            if (!TryParseGesture(gestureString, out var modifiers, out var key))
                return;

            lock (_lock)
            {
                _byName[name] = new Hotkey(modifiers, key, callback);
                EnsureHook();
            }
        }

        /// <summary>
        /// Unregister a hotkey by name.
        /// </summary>
        public static void Unregister(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            lock (_lock)
                _byName.Remove(name);
        }

        /// <summary>
        /// Stop and dispose the global hook. SharpHook's <c>RunAsync</c> runs the
        /// native event loop on a thread that lives for the process lifetime, so it
        /// must be disposed explicitly for the process to be able to exit.
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                _byName.Clear();
                if (_hook != null)
                {
                    _hook.KeyPressed -= OnKeyPressed;
                    try { _hook.Dispose(); } catch { }
                    _hook = null;
                }
            }
        }

        private static void EnsureHook()
        {
            if (_hook != null) return;

            _hook = new EventLoopGlobalHook();
            _hook.KeyPressed += OnKeyPressed;
            // Fire and forget: the hook runs on its own thread for the lifetime of the
            // process. The process is terminated by the plugin when KeePass closes.
            _ = _hook.RunAsync();
        }

        private static void OnKeyPressed(object sender, KeyboardHookEventArgs e)
        {
            var pressed = ToMods(e.RawEvent.Mask);
            var key = e.Data.KeyCode;

            Action callback = null;
            lock (_lock)
            {
                foreach (var hotkey in _byName.Values)
                {
                    if (hotkey.Key == key && hotkey.Modifiers == pressed)
                    {
                        callback = hotkey.Callback;
                        break;
                    }
                }
            }

            if (callback != null)
            {
                // Consume the keystroke so it is not also delivered to the focused app.
                e.SuppressEvent = true;
                callback();
            }
        }

        private static Mods ToMods(EventMask mask)
        {
            var mods = Mods.None;
            if ((mask & EventMask.Ctrl) != 0) mods |= Mods.Ctrl;
            if ((mask & EventMask.Alt) != 0) mods |= Mods.Alt;
            if ((mask & EventMask.Shift) != 0) mods |= Mods.Shift;
            if ((mask & EventMask.Meta) != 0) mods |= Mods.Win;
            return mods;
        }

        private static bool TryParseGesture(string gesture, out Mods modifiers, out KeyCode key)
        {
            modifiers = Mods.None;
            key = KeyCode.VcUndefined;
            var hasKey = false;

            foreach (var rawPart in gesture.Split('+'))
            {
                var part = rawPart.Trim();
                if (part.Length == 0) continue;

                switch (part.ToLowerInvariant())
                {
                    case "ctrl":
                    case "control": modifiers |= Mods.Ctrl; break;
                    case "alt": modifiers |= Mods.Alt; break;
                    case "shift": modifiers |= Mods.Shift; break;
                    case "win":
                    case "meta":
                    case "cmd": modifiers |= Mods.Win; break;
                    default:
                        if (!TryParseKey(part, out key))
                            return false;
                        hasKey = true;
                        break;
                }
            }

            return hasKey;
        }

        private static bool TryParseKey(string token, out KeyCode key)
        {
            // Single letter or digit (A-Z, 0-9) -> VcA..VcZ / Vc0..Vc9
            if (token.Length == 1)
            {
                var c = char.ToUpperInvariant(token[0]);
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    return Enum.TryParse("Vc" + c, out key);
            }

            // Function keys F1..F24 and any other named key matching the KeyCode enum
            // suffix (e.g. "Enter", "Space", "Escape", "Tab").
            if (Enum.TryParse("Vc" + token, ignoreCase: true, out key))
                return true;

            key = KeyCode.VcUndefined;
            return false;
        }
    }
}
