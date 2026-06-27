// SPDX-FileCopyrightText: Copyright (C) 2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Platform
{
    internal enum ActivationAction
    {
        ShowPrimary, // --primary
        ShowCurrent, // --current (default)
        NewEntry,    // --new-entry
    }

    /// <summary>
    /// Cross-process activation for when the exe is launched directly instead of by the plugin.
    /// A plugin-launched instance owns one named auto-reset event per <see cref="ActivationAction"/>
    /// and listens on all of them; a direct launch signals the matching event, then exits.
    /// </summary>
    internal static class SingleInstance
    {
        private static readonly Dictionary<ActivationAction, string> EventNames =
            Enum.GetValues<ActivationAction>()
                .ToDictionary(action => action, action => $"FluentPassFinder.Activate.{action}");

        public static bool TrySignalRunningInstance(ActivationAction action)
        {
            try
            {
                using var handle = EventWaitHandle.OpenExisting(EventNames[action]);
                handle.Set();
                return true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Program.WriteLog("SingleInstanceSignal", ex.ToString());
                return false;
            }
        }

        public static void StartActivationListener(Action<ActivationAction> onActivate)
        {
            var actions = EventNames.Keys.ToArray();
            EventWaitHandle[] handles;
            try
            {
                handles = actions
                    .Select(action => new EventWaitHandle(false, EventResetMode.AutoReset, EventNames[action]))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Program.WriteLog("SingleInstanceListen", ex.ToString());
                return;
            }

            var thread = new Thread(() =>
            {
                while (true)
                {
                    var action = actions[WaitHandle.WaitAny(handles)];
                    try { onActivate(action); }
                    catch (Exception ex) { Program.WriteLog("SingleInstanceActivate", ex.ToString()); }
                }
            })
            { IsBackground = true, Name = "FluentPassFinder.Activation" };
            thread.Start();
        }

        // Returns false for a plugin launch (args[0] is the pipe name, not a flag).
        public static bool TryParseActivationAction(string[] args, out ActivationAction action)
        {
            action = ActivationAction.ShowCurrent;

            if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0]))
                return true;

            switch (args[0].ToLowerInvariant())
            {
                case "--primary": action = ActivationAction.ShowPrimary; return true;
                case "--current": action = ActivationAction.ShowCurrent; return true;
                case "--new-entry": action = ActivationAction.NewEntry; return true;
                default: return false;
            }
        }
    }
}
