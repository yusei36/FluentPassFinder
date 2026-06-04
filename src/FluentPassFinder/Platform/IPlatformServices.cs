// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;

namespace FluentPassFinder.Platform
{
    /// <summary>
    /// Abstraction over the OS-specific functionality the app relies on: global hotkey
    /// registration and foreground-window handling. Today only
    /// <see cref="WindowsPlatformServices"/> exists; Linux/macOS implementations can be
    /// added behind this interface without touching the views or view models.
    /// See docs/CrossPlatform.md.
    /// </summary>
    internal interface IPlatformServices
    {
        /// <summary>Register a global hotkey. Gesture format: "Ctrl+Alt+K", "Shift+Win+F1".</summary>
        void RegisterHotkey(string name, string gesture, Action callback);

        /// <summary>Unregister a previously registered hotkey by name.</summary>
        void UnregisterHotkey(string name);

        /// <summary>Tear down all hotkeys so the owning thread/loop can exit.</summary>
        void DisposeHotkeys();

        /// <summary>Current mouse cursor position in screen pixels.</summary>
        PixelPoint GetCursorPosition();

        /// <summary>
        /// Remove the OS-drawn border around the given native window. Cosmetic;
        /// implementations may no-op where unsupported or when <paramref name="windowHandle"/>
        /// is <see cref="IntPtr.Zero"/>.
        /// </summary>
        void RemoveWindowBorder(IntPtr windowHandle);

        /// <summary>
        /// Force the given native window to the foreground and give it keyboard focus,
        /// working around the OS focus-stealing prevention.
        /// </summary>
        void ForceForegroundWindow(IntPtr windowHandle);

        /// <summary>
        /// Begin watching for the foreground window changing to one owned by another
        /// process, invoking <paramref name="onForegroundChangedToOtherProcess"/> when it
        /// does. Idempotent; the watch lives for the process lifetime.
        /// </summary>
        void StartForegroundWatch(Action onForegroundChangedToOtherProcess);
    }
}
