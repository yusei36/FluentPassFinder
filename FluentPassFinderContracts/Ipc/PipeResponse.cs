namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class PipeResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }

        public string StringValue { get; set; }
        public bool? BoolValue { get; set; }
        public EntryDto[] Entries { get; set; }
        public Settings Settings { get; set; }
    }
}
