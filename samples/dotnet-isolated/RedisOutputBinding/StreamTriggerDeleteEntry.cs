using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class StreamTriggerDeleteEntry
    {
        private readonly ILogger<StreamTriggerDeleteEntry> logger;

        public StreamTriggerDeleteEntry(ILogger<StreamTriggerDeleteEntry> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(StreamTriggerDeleteEntry))]
        [RedisOutput(Common.localhostSetting, "XDEL")]
        public string[] Run(
            [RedisStreamTrigger(Common.localhostSetting, "streamTest2")] StreamEntry entry)
        {
            logger.LogInformation($"Stream entry from key 'streamTest2' with Id '{entry.Id}' and values '" +
                JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString())));
            return new string[] { "streamTest2", entry.Id.ToString() };
        }
    }
}
