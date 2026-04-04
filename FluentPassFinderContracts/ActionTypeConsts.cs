using System;
using System.Linq;

namespace FluentPassFinder.Contracts.Public
{
    public static class ActionTypeConsts
    {
        public static string OpenContextMenu = nameof(OpenContextMenu);
        public static string SelectEntry = nameof(SelectEntry);
        public static string OpenUrl = nameof(OpenUrl);
        public static string AutoType = nameof(AutoType);
        public static string CopyActionPattern = "Copy_{0}";
        public static string AutoTypeActionPattern = "AutoType_{0}";

        public static string Totp = nameof(Totp);

        /// <summary>The five standard KeePass field names.</summary>
        public static readonly string[] StandardFieldNames = { "Title", "UserName", "Password", "Notes", "URL" };

        public static bool IsStandardField(string fieldName) =>
            StandardFieldNames.Any(f => f.Equals(fieldName, StringComparison.Ordinal));
    }
}
