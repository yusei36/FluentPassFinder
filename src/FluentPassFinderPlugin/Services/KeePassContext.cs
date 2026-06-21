// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Shared access to the hosting KeePass instance for the request services: UI-thread
    /// marshalling, database lookup, and UUID conversion. KeePass API calls must run on its UI
    /// thread, so every service routes through <see cref="Invoke(Action)"/> / <see cref="Invoke{T}(Func{T})"/>.
    /// </summary>
    internal class KeePassContext
    {
        private readonly IPluginHost pluginHost;

        public MainForm MainWindow { get; }

        public KeePassContext(IPluginHost pluginHost)
        {
            this.pluginHost = pluginHost;
            MainWindow = pluginHost.MainWindow;
        }

        /// <summary>The currently active database (may be null or closed).</summary>
        public PwDatabase ActiveDatabase => pluginHost.Database;

        public IEnumerable<PwDatabase> OpenDatabases => MainWindow.DocumentManager.GetOpenDatabases();

        public void Invoke(Action action) => MainWindow.Invoke(action);

        public T Invoke<T>(Func<T> func) => (T)MainWindow.Invoke(func);

        public (PwEntry entry, PwDatabase database) ResolveEntry(string entryUuid, string databaseUuid)
        {
            if (string.IsNullOrEmpty(entryUuid) || string.IsNullOrEmpty(databaseUuid))
                return (null, null);

            var dbUuidObj = new PwUuid(Convert.FromBase64String(databaseUuid));
            var database = OpenDatabases.FirstOrDefault(db => db.RootGroup.Uuid.Equals(dbUuidObj));
            if (database == null) return (null, null);

            var entryUuidObj = new PwUuid(Convert.FromBase64String(entryUuid));
            return (database.RootGroup.FindEntry(entryUuidObj, true), database);
        }

        public static string UuidToString(PwUuid uuid) =>
            Convert.ToBase64String(uuid.UuidBytes);
    }
}
