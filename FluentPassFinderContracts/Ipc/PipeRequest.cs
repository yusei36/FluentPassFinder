using Newtonsoft.Json;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    [JsonConverter(typeof(PipeRequestConverter))]
    public abstract class PipeRequest
    {
        public string Id { get; set; }
        public abstract string Type { get; }
    }
}
