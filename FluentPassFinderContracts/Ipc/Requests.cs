namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesRequest : PipeRequest
    {
        public string Query { get; set; }
    }

    public class GetPlaceholderValueRequest : PipeRequest
    {
        public string Placeholder { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public bool ResolveAll { get; set; }
    }

    public class GetStringFromCustomConfigRequest : PipeRequest
    {
        public string ConfigId { get; set; }
        public string DefaultValue { get; set; }
    }

    public class GetSettingsRequest : PipeRequest { }

    public class IsAnyDatabaseOpenRequest : PipeRequest { }

    public class CopyFieldRequest : PipeRequest
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class CopyToClipboardRequest : PipeRequest
    {
        public string Value { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class AutoTypeFieldRequest : PipeRequest
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class PerformAutoTypeRequest : PipeRequest
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string Sequence { get; set; }
    }

    public class OpenEntryUrlRequest : PipeRequest
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class SelectEntryRequest : PipeRequest
    {
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }
}
