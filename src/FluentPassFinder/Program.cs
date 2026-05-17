// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Controls;
using System.IO;

namespace FluentPassFinder
{
    internal static class Program
    {
        private static readonly string LogPath =
            Path.Combine(Path.GetTempPath(), "FluentPassFinder.error.log");

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                WriteLog("UnhandledException", e.ExceptionObject?.ToString());

            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
            catch (Exception ex)
            {
                WriteLog("FatalException", ex.ToString());
            }
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                      .UsePlatformDetect()
                      .LogToTrace();

        internal static void WriteLog(string label, string message)
        {
            try
            {
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {label}:{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
