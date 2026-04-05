namespace FluentPassFinder.Contracts.Public.Ipc
{
    /// <summary>Base class for all pipe request messages.</summary>
    public class PipeRequest
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }
}
