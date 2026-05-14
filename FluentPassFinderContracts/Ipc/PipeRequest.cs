using Newtonsoft.Json;

namespace FluentPassFinder.Contracts.Public.Ipc
{
    [JsonConverter(typeof(PipeRequestConverter))]
    public abstract class PipeRequest
    {
        public abstract string Type { get; }
    }
}
