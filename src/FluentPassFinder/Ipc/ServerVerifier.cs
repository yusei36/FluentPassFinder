// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FluentPassFinder.Ipc
{
    /// <summary>
    /// Verifies the pipe is served by the KeePass process that spawned this app, not a squatter.
    /// Matches the exact host PID when known, else falls back to an image-name check.
    /// </summary>
    internal static class ServerVerifier
    {
        private const string ExpectedServerImage = "KeePass.exe";

        public static bool VerifyServer(SafePipeHandle pipeHandle, int? expectedHostPid, out string reason)
        {
            reason = null;
            try
            {
                if (!GetNamedPipeServerProcessId(pipeHandle, out var serverPid))
                {
                    reason = "Failed to get server process ID";
                    return false;
                }

                if (expectedHostPid.HasValue)
                {
                    if (serverPid != (uint)expectedHostPid.Value)
                    {
                        reason = $"Server PID {serverPid} does not match expected host PID {expectedHostPid.Value}";
                        return false;
                    }
                    return true;
                }

                var imagePath = GetProcessImagePath(serverPid);
                if (imagePath == null)
                {
                    reason = "Failed to get server process image path";
                    return false;
                }

                var fileName = Path.GetFileName(imagePath);
                if (!string.Equals(fileName, ExpectedServerImage, StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"Unexpected server process: {fileName}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason = $"Verification error: {ex.Message}";
                return false;
            }
        }

        private static string GetProcessImagePath(uint pid)
        {
            var hProcess = OpenProcess(ProcessQueryLimitedInformation, false, pid);
            if (hProcess == IntPtr.Zero)
                return null;
            try
            {
                var sb = new StringBuilder(1024);
                var size = sb.Capacity;
                if (!QueryFullProcessImageName(hProcess, 0, sb, ref size))
                    return null;
                return sb.ToString(0, size);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        private const uint ProcessQueryLimitedInformation = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeServerProcessId(SafePipeHandle Pipe, out uint ServerProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);
    }
}
