namespace FluentPassFinder.Contracts.Public.Ipc
{

    public static class PipeRequestTypes
    {
        public const string SearchEntries             = nameof(SearchEntries);
        public const string GetPlaceholderValue       = nameof(GetPlaceholderValue);
        public const string GetStringFromCustomConfig = nameof(GetStringFromCustomConfig);
        public const string GetSettings              = nameof(GetSettings);
        public const string IsAnyDatabaseOpen         = nameof(IsAnyDatabaseOpen);
        public const string CopyField                = nameof(CopyField);
        public const string CopyToClipboard          = nameof(CopyToClipboard);
        public const string AutoTypeField            = nameof(AutoTypeField);
        public const string PerformAutoType          = nameof(PerformAutoType);
        public const string OpenEntryUrl             = nameof(OpenEntryUrl);
        public const string SelectEntry              = nameof(SelectEntry);
        public const string SaveSettings             = nameof(SaveSettings);
    }
}
