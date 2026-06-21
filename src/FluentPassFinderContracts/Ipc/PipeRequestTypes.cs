// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
namespace FluentPassFinder.Contracts.Public.Ipc
{
    public static class PipeRequestTypes
    {
        public const string SearchEntries             = "SearchEntries";
        public const string HasTotp                   = "HasTotp";
        public const string GetSettings               = "GetSettings";
        public const string IsAnyDatabaseOpen         = "IsAnyDatabaseOpen";
        public const string CopyField                 = "CopyField";
        public const string CopyToClipboard           = "CopyToClipboard";
        public const string AutoTypeField             = "AutoTypeField";
        public const string PerformAutoType           = "PerformAutoType";
        public const string OpenEntryUrl              = "OpenEntryUrl";
        public const string SelectEntry               = "SelectEntry";
        public const string SaveSettings              = "SaveSettings";
        public const string GetTemplates              = "GetTemplates";
        public const string CreateEntry               = "CreateEntry";
        public const string GeneratePassword          = "GeneratePassword";
    }
}
