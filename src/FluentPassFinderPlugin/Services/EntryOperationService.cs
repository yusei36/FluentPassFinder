// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using FluentPassFinder.Contracts.Public.Ipc;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using System;

namespace FluentPassFinder.Services
{
    /// <summary>
    /// Performs actions against an existing entry resolved from the request's UUIDs: copy a
    /// field or arbitrary value to the clipboard, auto-type, open the entry URL, select the
    /// entry in KeePass, and probe for a TOTP placeholder.
    /// </summary>
    internal class EntryOperationService
    {
        private readonly KeePassContext context;

        public EntryOperationService(KeePassContext context)
        {
            this.context = context;
        }

        public HasTotpResponse HasTotp(HasTotpRequest req)
        {
            var hasTotp = context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return false;
                var value = SprEngine.Compile(req.Placeholder, new SprContext(entry, db, SprCompileFlags.All, true, false));
                return !string.IsNullOrEmpty(value) && value != req.Placeholder;
            });
            return new HasTotpResponse { Success = true, HasTotp = hasTotp };
        }

        public PipeResponse CopyField(CopyFieldRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    context.MainWindow.StartClipboardCountdown();
            });
            return Ack();
        }

        public PipeResponse CopyToClipboard(CopyToClipboardRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = req.Value;
                if (!string.IsNullOrEmpty(value) && value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.All, true, false));

                if (ClipboardUtil.Copy(value, false, true, entry, db, IntPtr.Zero))
                    context.MainWindow.StartClipboardCountdown();
            });
            return Ack();
        }

        public PipeResponse AutoTypeField(AutoTypeFieldRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var value = entry.Strings.ReadSafe(req.FieldName);
                if (value.IndexOf('{') >= 0)
                    value = SprEngine.Compile(value, new SprContext(entry, db, SprCompileFlags.Deref, true, false));

                AutoType.PerformIntoCurrentWindow(entry, db, value + "{ENTER}");
            });
            return Ack();
        }

        public PipeResponse PerformAutoType(PerformAutoTypeRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null)
                    AutoType.PerformIntoCurrentWindow(entry, db, req.Sequence);
            });
            return Ack();
        }

        public PipeResponse OpenEntryUrl(OpenEntryUrlRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, _) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry != null) WinUtil.OpenEntryUrl(entry);
            });
            return Ack();
        }

        public PipeResponse SelectEntry(SelectEntryRequest req)
        {
            context.Invoke(() =>
            {
                var (entry, db) = context.ResolveEntry(req.EntryUuid, req.DatabaseUuid);
                if (entry == null) return;

                var mainWindow = context.MainWindow;
                mainWindow.UpdateUI(false, mainWindow.DocumentManager.FindDocument(db), true, entry.ParentGroup, true, null, false);
                mainWindow.SelectEntries(new PwObjectList<PwEntry> { entry }, true, true);
                mainWindow.EnsureVisibleEntry(entry.Uuid);
                mainWindow.UpdateUI(false, null, false, null, false, null, false);
                mainWindow.EnsureVisibleForegroundWindow(true, true);
            });
            return Ack();
        }

        private static PipeResponse Ack() =>
            new PipeResponse { Success = true };
    }
}
