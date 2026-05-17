// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinderPlugin.Ipc;
using KeePass.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FluentPassFinderPlugin
{
    public sealed class FluentPassFinderPluginExt : Plugin
    {
        private const string applicationExeName = "FluentPassFinder.exe";
        private Process appProcess;
        private PipeServer pipeServer;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;

            var handler = new PluginRequestHandler(host);
            var pipeName = "FluentPassFinder_" + Guid.NewGuid().ToString("N");

            pipeServer = new PipeServer(pipeName, handler);
            pipeServer.Start();

            var hostPid = Process.GetCurrentProcess().Id;
            appProcess = Process.Start(FindAppExePath(), $"{pipeName} {hostPid}");

            return true;
        }

        public override void Terminate()
        {
            try
            {
                if (appProcess != null && !appProcess.HasExited)
                    appProcess.Kill();
            }
            catch (Exception) { }

            pipeServer?.Dispose();

            base.Terminate();
        }

        private string FindAppExePath()
        {
            var pluginAssembly = Assembly.GetAssembly(typeof(FluentPassFinderPluginExt));
            var fileInfo = new FileInfo(pluginAssembly.Location);
            var files = Directory.GetFiles(fileInfo.Directory.FullName, applicationExeName, SearchOption.AllDirectories);

            if (!files.Any())
                throw new Exception($"{applicationExeName} was not found.");

            return files[0];
        }
    }
}
