// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Ipc;
using KeePass.Plugins;
using System;
using System.Diagnostics;
using System.Drawing;
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

        // Loaded once; MemoryStream kept open for GDI+ lifetime requirement.
        private static readonly Image _smallIcon = LoadSmallIcon();

        public override string UpdateUrl => "https://raw.githubusercontent.com/yusei36/FluentPassFinder/refs/heads/master/version.txt";

        public override Image SmallIcon => _smallIcon;

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

        private static Image LoadSmallIcon()
        {
            var asm = Assembly.GetExecutingAssembly();
            using (Stream raw = asm.GetManifestResourceStream(
                "FluentPassFinder.Resources.plugin-icon.png"))
            {
                var ms = new MemoryStream();
                raw.CopyTo(ms);
                ms.Position = 0;
                return Image.FromStream(ms);
            }
        }

        private string FindAppExePath()
        {
            // exe needs to sit next to the plugin DLL or in a subdirectory, otherwise the app won't be able to find the plugin DLL
            var pluginDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FluentPassFinderExt)).Location);
            var sameDirPath = Path.Combine(pluginDir, applicationExeName);
            if (File.Exists(sameDirPath))
                return sameDirPath;

            var files = Directory.GetFiles(pluginDir, applicationExeName, SearchOption.AllDirectories);
            if (files.Any())
                return files[0];

            throw new Exception($"{applicationExeName} was not found.");
        }
    }
}
