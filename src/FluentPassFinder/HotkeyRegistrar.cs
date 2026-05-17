// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FluentPassFinder
{
    /// <summary>
    /// Registers global hotkeys via Win32 RegisterHotKey using a hidden message-only window
    /// on a dedicated STA background thread. No WinForms or WPF dependency required.
    /// </summary>
    internal static class HotkeyRegistrar
    {
        private static readonly Dictionary<int, Action> _handlers = new();
        private static readonly Dictionary<string, int> _nameToId = new();
        private static readonly Queue<(string name, string gesture, Action callback)> _registerQueue = new();
        private static readonly Queue<string> _unregisterQueue = new();
        private static readonly object _queueLock = new();
        private static int _nextId = 0xC000;

        private static volatile IntPtr _hwnd = IntPtr.Zero;
        private static readonly ManualResetEventSlim _hwndReady = new(false);
        private static bool _loopStarted;
        private static readonly object _loopLock = new();

        private const int WM_HOTKEY = 0x0312;
        private const uint WM_APP_REGISTER = 0x8001; // signal loop thread to drain queue

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static readonly IntPtr HWND_MESSAGE = new(-3);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX, ptY;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Keep delegate alive to prevent GC collection
        private static WndProcDelegate _wndProcDelegate;

        /// <summary>
        /// Register a global hotkey. Gesture format: "Ctrl+Alt+K", "Shift+Win+F1", etc.
        /// Safe to call multiple times from any thread.
        /// </summary>
        public static void Register(string name, string gestureString, Action callback)
        {
            if (string.IsNullOrWhiteSpace(gestureString))
                return;

            lock (_queueLock)
                _registerQueue.Enqueue((name, gestureString, callback));

            EnsureMessageLoop();

            // Signal the message loop thread to drain the queue.
            // _hwnd is set before _hwndReady fires, so this is safe.
            PostMessage(_hwnd, WM_APP_REGISTER, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Unregister a hotkey by name. The unregistration is queued and processed on the
        /// message loop thread (same thread that called RegisterHotKey) before any pending
        /// registrations, so calling Unregister then Register with the same name is safe.
        /// </summary>
        public static void Unregister(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            lock (_queueLock)
                _unregisterQueue.Enqueue(name);

            EnsureMessageLoop();
            PostMessage(_hwnd, WM_APP_REGISTER, IntPtr.Zero, IntPtr.Zero);
        }

        private static void EnsureMessageLoop()
        {
            lock (_loopLock)
            {
                if (_loopStarted) return;
                _loopStarted = true;

                var thread = new Thread(RunMessageLoop) { IsBackground = true };
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            _hwndReady.Wait();
        }

        private static void RunMessageLoop()
        {
            _wndProcDelegate = WndProc;

            var className = "FluentPassFinder_HotkeyWnd";
            var wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                lpfnWndProc = _wndProcDelegate,
                hInstance = GetModuleHandle(null),
                lpszClassName = className,
            };
            RegisterClassEx(ref wc);

            _hwnd = CreateWindowEx(0, className, string.Empty, 0,
                0, 0, 0, 0,
                HWND_MESSAGE, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);

            _hwndReady.Set(); // unblock any callers waiting in EnsureMessageLoop

            while (GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_HOTKEY)
            {
                var id = wParam.ToInt32();
                if (_handlers.TryGetValue(id, out var handler))
                    handler?.Invoke();
                return IntPtr.Zero;
            }

            if (msg == WM_APP_REGISTER)
            {
                // Drain both queues on the message loop thread so that RegisterHotKey /
                // UnregisterHotKey are always called from the thread that owns the hotkeys.
                // Unregistrations are processed first so re-registering with the same name works.
                lock (_queueLock)
                {
                    while (_unregisterQueue.Count > 0)
                    {
                        var name = _unregisterQueue.Dequeue();
                        if (_nameToId.TryGetValue(name, out var id))
                        {
                            UnregisterHotKey(_hwnd, id);
                            _handlers.Remove(id);
                            _nameToId.Remove(name);
                        }
                    }

                    while (_registerQueue.Count > 0)
                    {
                        var (name, gesture, callback) = _registerQueue.Dequeue();
                        DoRegister(name, gesture, callback);
                    }
                }
                return IntPtr.Zero;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private static void DoRegister(string name, string gestureString, Action callback)
        {
            var (modifiers, vk) = ParseGesture(gestureString);
            if (vk == 0) return;

            var id = _nextId++;
            _handlers[id] = callback;
            if (name != null) _nameToId[name] = id;
            RegisterHotKey(_hwnd, id, modifiers | MOD_NOREPEAT, vk);
        }

        private static (uint modifiers, uint vk) ParseGesture(string gesture)
        {
            uint modifiers = 0;
            uint vk = 0;

            foreach (var part in gesture.Split('+'))
            {
                switch (part.Trim().ToLowerInvariant())
                {
                    case "ctrl":  modifiers |= MOD_CONTROL; break;
                    case "alt":   modifiers |= MOD_ALT;     break;
                    case "shift": modifiers |= MOD_SHIFT;   break;
                    case "win":   modifiers |= MOD_WIN;     break;
                    default:
                        var trimmed = part.Trim();
                        if (trimmed.Length == 1)
                            vk = (uint)char.ToUpper(trimmed[0]);
                        break;
                }
            }

            return (modifiers, vk);
        }
    }
}
