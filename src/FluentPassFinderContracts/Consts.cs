// SPDX-FileCopyrightText: Copyright (C) 2023-2026 Uwe Koegel
// SPDX-License-Identifier: GPL-3.0-or-later
using System;
using System.Linq;

namespace FluentPassFinder.Contracts.Public
{
    public static class Consts
    {
        public static string OpenContextMenu = nameof(OpenContextMenu);
        public static string SelectEntry = nameof(SelectEntry);
        public static string CreateFromTemplate = nameof(CreateFromTemplate);
        public static string OpenUrl = nameof(OpenUrl);
        public static string AutoType = nameof(AutoType);
        public static string Totp = nameof(Totp);
        public static string CopyActionPattern = "Copy_{0}";
        public static string AutoTypeActionPattern = "AutoType_{0}";

        public static readonly string[] StandardFieldNames = { TitleField, UserNameField, PasswordField, NotesField, UrlField };
        
        public const string TitleField = "Title";
        public const string UserNameField = "UserName";
        public const string PasswordField = "Password";
        public const string NotesField = "Notes";
        public const string UrlField = "URL";

        public const string TemplateUuidField = "_etm_template_uuid";
        public const string NativeTotpPlaceholder = "{TIMEOTP}";

        public const string DefaultNewEntryGroupUuid = "A436B624EE2C4421B3E0949924E9C18C";
        public const string DefaultNewEntryGroupName = "New entries";

        public const string AutoTypeEnterPlaceholder = "{ENTER}";

        public static bool IsStandardField(string fieldName) =>
            StandardFieldNames.Any(f => f.Equals(fieldName, StringComparison.Ordinal));
    }
}
