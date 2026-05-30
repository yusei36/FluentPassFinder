// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.SearchEntries;
        public EntryDto[] Entries { get; set; }
    }

    public class HasTotpResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.HasTotp;
        public bool HasTotp { get; set; }
    }

    public class GetSettingsResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.GetSettings;
        public Settings Settings { get; set; }
    }

    public class IsAnyDatabaseOpenResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.IsAnyDatabaseOpen;
        public bool IsOpen { get; set; }
    }

    // Void action responses (CopyField, AutoTypeField, CopyToClipboard, PerformAutoType,
    // OpenEntryUrl, SelectEntry) use the base PipeResponse directly; Success/Error is enough.
}
