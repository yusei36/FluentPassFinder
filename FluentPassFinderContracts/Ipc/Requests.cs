namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class SearchEntriesRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.SearchEntries;
        public string Query { get; set; }
    }

    public class GetPlaceholderValueRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.GetPlaceholderValue;
        public string Placeholder { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public bool ResolveAll { get; set; }
    }

    public class GetStringFromCustomConfigRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.GetStringFromCustomConfig;
        public string ConfigId { get; set; }
        public string DefaultValue { get; set; }
    }

    public class GetSettingsRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.GetSettings;
    }

    public class IsAnyDatabaseOpenRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.IsAnyDatabaseOpen;
    }

    public class CopyFieldRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.CopyField;
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class CopyToClipboardRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.CopyToClipboard;
        public string Value { get; set; }
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class AutoTypeFieldRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.AutoTypeField;
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string FieldName { get; set; }
    }

    public class PerformAutoTypeRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.PerformAutoType;
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
        public string Sequence { get; set; }
    }

    public class OpenEntryUrlRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.OpenEntryUrl;
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class SelectEntryRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.SelectEntry;
        public string EntryUuid { get; set; }
        public string DatabaseUuid { get; set; }
    }

    public class SaveSettingsRequest : PipeRequest
    {
        public override string Type => PipeRequestTypes.SaveSettings;
        public Settings Settings { get; set; }
    }
}
