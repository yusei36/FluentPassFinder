namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesRequest : PipeEnvelope
    {
        public string Query { get; set; }
    }

    public class GetPlaceholderValueRequest : PipeEnvelope
    {
        public string Placeholder { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public bool ResolveAll { get; set; }
    }

    public class GetStringFromCustomConfigRequest : PipeEnvelope
    {
        public string ConfigId { get; set; }
        public string DefaultValue { get; set; }
    }

    public class GetSettingsRequest : PipeEnvelope { }

    public class IsAnyDatabaseOpenRequest : PipeEnvelope { }

    public class CopyFieldRequest : PipeEnvelope
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class CopyToClipboardRequest : PipeEnvelope
    {
        public string Value { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class AutoTypeFieldRequest : PipeEnvelope
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class PerformAutoTypeRequest : PipeEnvelope
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string Sequence { get; set; }
    }

    public class OpenEntryUrlRequest : PipeEnvelope
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class SelectEntryRequest : PipeEnvelope
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }
}
