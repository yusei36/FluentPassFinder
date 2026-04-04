namespace FluentPassFinder.Contracts.Public.Ipc
{
    /// <summary>
    /// Base class for every message sent over the named pipe.
    /// Requests inherit and add their parameters; responses inherit and add their return values.
    /// </summary>
    public class PipeEnvelope
    {
        public string Id { get; set; }
        public string Type { get; set; }

        // Populated on responses only
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
    }
}
