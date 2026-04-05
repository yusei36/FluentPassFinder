namespace FluentPassFinder.Contracts.Public.Ipc
{
    /// <summary>Base class for all pipe response messages.</summary>
    public class PipeResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
