namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesResponse : PipeResponse
    {
        public EntryDto[] Entries { get; set; }
    }

    public class GetPlaceholderValueResponse : PipeResponse
    {
        public string Value { get; set; }
    }

    public class GetStringFromCustomConfigResponse : PipeResponse
    {
        public string Value { get; set; }
    }

    public class GetSettingsResponse : PipeResponse
    {
        public Settings Settings { get; set; }
    }

    public class IsAnyDatabaseOpenResponse : PipeResponse
    {
        public bool IsOpen { get; set; }
    }

    // Void action responses (CopyField, AutoTypeField, CopyToClipboard, PerformAutoType,
    // OpenEntryUrl, SelectEntry) use the base PipeResponse directly — Success/Error is enough.
}
