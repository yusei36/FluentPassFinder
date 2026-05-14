namespace FluentPassFinder.Contracts.Public.Ipc
{
    public class PipeResponse
    {
        public virtual string Type => null;
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
