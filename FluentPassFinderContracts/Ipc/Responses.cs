namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.SearchEntries;
        public EntryDto[] Entries { get; set; }
    }

    public class GetPlaceholderValueResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.GetPlaceholderValue;
        public string Value { get; set; }
    }

    public class GetStringFromCustomConfigResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.GetStringFromCustomConfig;
        public string Value { get; set; }
    }

    public class GetSettingsResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.GetSettings;
        public Settings Settings { get; set; }
    }

    public class IsAnyDatabaseOpenResponse : PipeResponse
    {
        public override string Type => PipeRequestTypes.IsAnyDatabaseOpen;
        public bool IsOpen { get; set; }
    }

    // Void action responses (CopyField, AutoTypeField, CopyToClipboard, PerformAutoType,
    // OpenEntryUrl, SelectEntry) use the base PipeResponse directly — Success/Error is enough.
}
