// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Ipc;
using KeePass.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FluentPassFinder
{
    public sealed class FluentPassFinderExt : Plugin
    {
        private const string applicationExeName = "FluentPassFinder.exe";
        private Process appProcess;
        private PipeServer pipeServer;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;

            var handler = new PluginRequestHandler(host);
            var pipeName = "FluentPassFinder_" + Guid.NewGuid().ToString("N");
            var appExePath = FindAppExePath();

            pipeServer = new PipeServer(pipeName, handler, appExePath);
            pipeServer.Start();

            var hostPid = Process.GetCurrentProcess().Id;
            appProcess = Process.Start(appExePath, $"{pipeName} {hostPid}");

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
            var pluginDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FluentPassFinderExt)).Location);

            // Installed layout: exe sits next to the plugin DLL
            var sameDirPath = Path.Combine(pluginDir, applicationExeName);
            if (File.Exists(sameDirPath))
                return sameDirPath;

            // Local dev layout: Debug build copies the app into a bin/ subdirectory
            var files = Directory.GetFiles(pluginDir, applicationExeName, SearchOption.AllDirectories);
            if (files.Any())
                return files[0];

            throw new Exception($"{applicationExeName} was not found.");
        }
    }
}
