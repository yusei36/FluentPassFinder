// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public.Ipc;
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace FluentPassFinder.Ipc
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
            serverStream = CreateServerStream();

            readerThread = new Thread(ReadLoop) { IsBackground = true, Name = "FluentPassFinder.PipeServer" };
            readerThread.Start();
        }

        private NamedPipeServerStream CreateServerStream()
        {
            // Most restrictive option the OS accepts wins; fallbacks keep the plugin loading.
            foreach (var security in new[] { BuildPipeSecurity(withLabel: true), BuildPipeSecurity(withLabel: false), null })
            {
                try
                {
                    return security != null
                        ? new NamedPipeServerStream(
                            pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                            PipeOptions.None, 0, 0, security)
                        : new NamedPipeServerStream(
                            pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);
                }
                catch (Exception) { }
            }

            return new NamedPipeServerStream(
                pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);
        }

        // Current user + SYSTEM only; withLabel also blocks lower-integrity same-user processes.
        private static PipeSecurity BuildPipeSecurity(bool withLabel)
        {
            try
            {
                var userSid = WindowsIdentity.GetCurrent().User.Value;
                var sddl = withLabel
                    ? $"D:(A;;GA;;;{userSid})(A;;GA;;;SY)S:(ML;;NRNW;;;ME)"
                    : $"D:(A;;GA;;;{userSid})(A;;GA;;;SY)";

                var rsd = new RawSecurityDescriptor(sddl);
                var bytes = new byte[rsd.BinaryLength];
                rsd.GetBinaryForm(bytes, 0);

                var security = new PipeSecurity();
                security.SetSecurityDescriptorBinaryForm(bytes, AccessControlSections.All);
                return security;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReadLoop()
        {
            while (running)
            {
                try
                {
                    serverStream.WaitForConnection();
                }
                catch (Exception)
                {
                    break; // stream disposed on shutdown
                }

                if (!running) break;

                try
                {
                    // Drop unauthorized clients and re-accept, so a rogue process winning the
                    // connect race cannot permanently deny service to the real app.
                    if (IsClientAuthorized())
                    {
                        while (running && serverStream.IsConnected)
                        {
                            var request = PipeProtocol.ReadRequest(serverStream);
                            if (request == null) break;

                            var response = handler.Handle(request);
                            PipeProtocol.WriteResponse(serverStream, response);
                        }
                    }
                }
                catch (Exception) { }

                try
                {
                    if (serverStream.IsConnected)
                        serverStream.Disconnect();
                }
                catch (Exception) { }
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
