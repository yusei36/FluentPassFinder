namespace FluentPassFinder.Contracts.Public.Ipc
{
    /// <summary>Base class for all pipe request messages.</summary>
    public class PipeRequest
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    /// <summary>Base class for all pipe response messages.</summary>
    public class PipeResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

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
