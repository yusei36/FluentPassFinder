// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public.Ipc;
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FluentPassFinderPlugin.Ipc
{
    internal class PipeServer : IDisposable
    {
        private readonly string pipeName;
        private readonly PluginRequestHandler handler;
        private readonly string expectedClientExe;
        private NamedPipeServerStream serverStream;
        private Thread readerThread;
        private volatile bool running;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out uint ClientProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);

        private const uint ProcessQueryLimitedInformation = 0x1000;

        public PipeServer(string pipeName, PluginRequestHandler handler, string expectedClientExe)
        {
            this.pipeName = pipeName;
            this.handler = handler;
            this.expectedClientExe = expectedClientExe;
        }

        public void Start()
        {
            running = true;
            serverStream = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.None);

            readerThread = new Thread(ReadLoop) { IsBackground = true, Name = "FluentPassFinder.PipeServer" };
            readerThread.Start();
        }

        private void ReadLoop()
        {
            try
            {
                serverStream.WaitForConnection();

                if (!IsClientAuthorized())
                {
                    serverStream.Disconnect();
                    return;
                }

                while (running && serverStream.IsConnected)
                {
                    var request = PipeProtocol.ReadRequest(serverStream);
                    if (request == null) break;

                    var response = handler.Handle(request);
                    PipeProtocol.WriteResponse(serverStream, response);
                }
            }
            catch (Exception)
            {
                // Connection closed or plugin shutting down — exit silently
            }
        }

        private bool IsClientAuthorized()
        {
            if (!GetNamedPipeClientProcessId(serverStream.SafePipeHandle.DangerousGetHandle(), out var clientPid))
                return false;

            var hProcess = OpenProcess(ProcessQueryLimitedInformation, false, clientPid);
            if (hProcess == IntPtr.Zero)
                return false;

            try
            {
                var sb = new StringBuilder(32767);
                uint size = (uint)sb.Capacity;
                if (!QueryFullProcessImageName(hProcess, 0, sb, ref size))
                    return false;

                return string.Equals(sb.ToString(), expectedClientExe, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        public void Dispose()
        {
            running = false;
            try { serverStream?.Dispose(); } catch { }
        }
    }
}
