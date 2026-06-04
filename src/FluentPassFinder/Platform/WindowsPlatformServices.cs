// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using System.Runtime.InteropServices;

namespace FluentPassFinder.Platform
{
    /// <summary>
    /// Windows implementation of <see cref="IPlatformServices"/>. Wraps Win32 P/Invoke for
    /// cursor position, foreground-window handling and the DWM border tweak, and delegates
    /// global hotkeys to <see cref="HotkeyRegistrar"/>. This is the only place Win32 lives.
    /// </summary>
    internal sealed class WindowsPlatformServices : IPlatformServices
    {
        // ---- Global hotkeys -------------------------------------------------

        public void RegisterHotkey(string name, string gesture, Action callback)
            => HotkeyRegistrar.Register(name, gesture, callback);

        public void UnregisterHotkey(string name) => HotkeyRegistrar.Unregister(name);

        public void DisposeHotkeys() => HotkeyRegistrar.Dispose();

        // ---- Cursor ---------------------------------------------------------

        public PixelPoint GetCursorPosition()
        {
            GetCursorPos(out var pt);
            return new PixelPoint(pt.X, pt.Y);
        }

        // ---- Window border (DWM) -------------------------------------------

        /// <summary>
        /// Windows 11 draws a 1px border around every window (following the corner radius),
        /// which shows up even though we render our own borderless rounded surface. Setting
        /// the DWM border color to <c>DWMWA_COLOR_NONE</c> removes it. The call is a no-op on
        /// Windows 10 (the attribute is unsupported there).
        /// </summary>
        public void RemoveWindowBorder(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            int colorNone = unchecked((int)DWMWA_COLOR_NONE);
            DwmSetWindowAttribute(windowHandle, DWMWA_BORDER_COLOR, ref colorNone, sizeof(int));
        }

        // ---- Foreground window ---------------------------------------------

        /// <summary>
        /// Forces the given window to the foreground and gives it keyboard focus.
        ///
        /// The app runs as a separate process from the one the user is interacting with
        /// (browser, Explorer, ...). When the hotkey fires, that other process is the
        /// foreground app, so Windows' focus-stealing prevention blocks our
        /// <c>SetForegroundWindow</c> call: the window becomes visible but keyboard input
        /// keeps going to the previously focused app. The reliable workaround is to
        /// temporarily attach our UI thread's input queue to the foreground window's thread
        /// (<c>AttachThreadInput</c>), which lifts the restriction so <c>SetForegroundWindow</c>
        /// actually takes effect, then detach again.
        /// </summary>
        public void ForceForegroundWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            var foreground = GetForegroundWindow();
            if (foreground == windowHandle) return;

            uint foregroundThread = foreground != IntPtr.Zero
                ? GetWindowThreadProcessId(foreground, out _)
                : 0;
            uint currentThread = GetWindowThreadProcessId(windowHandle, out _);

            bool attached = foregroundThread != 0
                && foregroundThread != currentThread
                && AttachThreadInput(currentThread, foregroundThread, true);
            try
            {
                SetForegroundWindow(windowHandle);
            }
            finally
            {
                if (attached)
                    AttachThreadInput(currentThread, foregroundThread, false);
            }
        }

        private Action _onForegroundChangedToOtherProcess;
        private WinEventDelegate _foregroundWatchProc; // keep a reference so it is not GC'd
        private IntPtr _foregroundWatchHook;

        /// <summary>
        /// Installs a system-wide <c>EVENT_SYSTEM_FOREGROUND</c> hook that fires whenever the
        /// foreground window changes. Avalonia's Deactivated/LostFocus is unreliable for a
        /// borderless topmost launcher window, so we use this to detect focus leaving for
        /// another process. The process check keeps our own popups (context menu, settings
        /// pane) from triggering. The hook is installed on the calling (UI) thread, so its
        /// callback is marshalled back onto that thread's message loop. It is installed once
        /// and lives for the process lifetime (terminated when KeePass closes).
        /// </summary>
        public void StartForegroundWatch(Action onForegroundChangedToOtherProcess)
        {
            if (_foregroundWatchHook != IntPtr.Zero) return;

            _onForegroundChangedToOtherProcess = onForegroundChangedToOtherProcess;
            _foregroundWatchProc = OnForegroundChanged; // keep a reference so it is not GC'd
            _foregroundWatchHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _foregroundWatchProc, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        private void OnForegroundChanged(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;

            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == (uint)Environment.ProcessId) return; // our own window or popups

            _onForegroundChangedToOtherProcess?.Invoke();
        }

        // ---- P/Invoke -------------------------------------------------------

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_BORDER_COLOR = 34;
        private const uint DWMWA_COLOR_NONE = 0xFFFFFFFE;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        private delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }
    }
}
