using System;
using System.Linq;

namespace FluentPassFinder.Contracts.Public
{
    public static class Consts
    {
        // Constats for the Actions
        public static string OpenContextMenu = nameof(OpenContextMenu);
        public static string SelectEntry = nameof(SelectEntry);
        public static string OpenUrl = nameof(OpenUrl);
        public static string AutoType = nameof(AutoType);
        public static string Totp = nameof(Totp);
        public static string CopyActionPattern = "Copy_{0}";
        public static string AutoTypeActionPattern = "AutoType_{0}";

        /// <summary>The five standard KeePass field names.</summary>
        public static readonly string[] StandardFieldNames = { TitleField, UserNameField, PasswordField, NotesField, UrlField };
        
        public const string TitleField = "Title";
        public const string UserNameField = "UserName";
        public const string PasswordField = "Password";
        public const string NotesField = "Notes";
        public const string UrlField = "URL";

        public const string TemplateUuidField = "_etm_template_uuid";
        public const string NativeTotpFieldPrefix = "TimeOtp-";
        public const string NativeTotpPlacholder = "{TIMEOTP}";
        public const string PluginTotpPlaceholder = "{TOTP}";
        public const string PluginTotpFieldConfigKey = "totpsettings_stringname"; // Key to the configuration (in KeePass.config.xml) of the field which is used by the totp plugin (actual field should be something like 'TOTP Settings')

        public const string AutoTypeEnterPlaceholder = "{ENTER}";

        public static bool IsStandardField(string fieldName) =>
            StandardFieldNames.Any(f => f.Equals(fieldName, StringComparison.Ordinal));
    }
}
