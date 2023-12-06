using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class StreamTriggerDeleteEntry_String
    {
        [FunctionName(nameof(StreamTriggerDeleteEntry_String))]
        public static void Run(
            [RedisStreamTrigger(Common.localhostSetting, "streamTest2")] StreamEntry entry,
            [Redis(Common.localhostSetting, "XDEL")] out string result,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key 'streamTest2' with Id '{entry.Id}' and values '" +
                JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString())));
            result = "streamTest2 " + entry.Id.ToString();
        }
    }
}
