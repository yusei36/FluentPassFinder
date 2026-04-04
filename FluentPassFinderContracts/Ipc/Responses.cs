namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesResponse : PipeEnvelope
    {
        public EntryDto[] Entries { get; set; }
    }

    public class GetPlaceholderValueResponse : PipeEnvelope
    {
        public string Value { get; set; }
    }

    public class GetStringFromCustomConfigResponse : PipeEnvelope
    {
        public string Value { get; set; }
    }

    public class GetSettingsResponse : PipeEnvelope
    {
        public Settings Settings { get; set; }
    }

    public class IsAnyDatabaseOpenResponse : PipeEnvelope
    {
        public bool IsOpen { get; set; }
    }

    // Void action responses (CopyField, AutoTypeField, CopyToClipboard, PerformAutoType,
    // OpenEntryUrl, SelectEntry) use the base PipeEnvelope directly — Success/Error is enough.
}
