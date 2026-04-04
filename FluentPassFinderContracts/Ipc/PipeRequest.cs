namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class PipeRequest
    {
        public string Id { get; set; }
        public string Type { get; set; }

        // SearchEntries
        public string Query { get; set; }

        // GetPlaceholderValue
        public string Placeholder { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public bool? ResolveAll { get; set; }

        // GetStringFromCustomConfig
        public string ConfigId { get; set; }
        public string DefaultValue { get; set; }

        // CopyField / AutoTypeField
        public string FieldName { get; set; }

        // CopyToClipboard / PerformAutoType (when value is caller-supplied)
        public string Value { get; set; }
        public string Sequence { get; set; }
    }

    public static class PipeRequestTypes
    {
        public const string SearchEntries = nameof(SearchEntries);
        public const string GetPlaceholderValue = nameof(GetPlaceholderValue);
        public const string GetStringFromCustomConfig = nameof(GetStringFromCustomConfig);
        public const string GetSettings = nameof(GetSettings);
        public const string IsAnyDatabaseOpen = nameof(IsAnyDatabaseOpen);
        public const string CopyField = nameof(CopyField);
        public const string CopyToClipboard = nameof(CopyToClipboard);
        public const string AutoTypeField = nameof(AutoTypeField);
        public const string PerformAutoType = nameof(PerformAutoType);
        public const string OpenEntryUrl = nameof(OpenEntryUrl);
        public const string SelectEntry = nameof(SelectEntry);
    }
}
