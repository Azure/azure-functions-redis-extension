using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class StreamTriggerDeleteEntry
    {
        [Function(nameof(StreamTriggerDeleteEntry))]
        [RedisOutput(Common.connectionStringSetting, "XDEL")]
        public static string Run(
            [RedisStreamTrigger(Common.connectionStringSetting, "streamTest2")] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key 'streamTest2' with Id '{entry.Id}' and values '" +
                JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString())));
            return "streamTest2 " + entry.Id.ToString();
        }
    }
}
