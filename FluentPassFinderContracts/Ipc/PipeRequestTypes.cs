namespace FluentPassFinder.Contracts.Public.Ipc
{
    public static class PipeRequestTypes
    {
        public const string SearchEntries             = "SearchEntries";
        public const string GetPlaceholderValue       = "GetPlaceholderValue";
        public const string GetStringFromCustomConfig = "GetStringFromCustomConfig";
        public const string GetSettings               = "GetSettings";
        public const string IsAnyDatabaseOpen         = "IsAnyDatabaseOpen";
        public const string CopyField                 = "CopyField";
        public const string CopyToClipboard           = "CopyToClipboard";
        public const string AutoTypeField             = "AutoTypeField";
        public const string PerformAutoType           = "PerformAutoType";
        public const string OpenEntryUrl              = "OpenEntryUrl";
        public const string SelectEntry               = "SelectEntry";
        public const string SaveSettings              = "SaveSettings";
    }
}
