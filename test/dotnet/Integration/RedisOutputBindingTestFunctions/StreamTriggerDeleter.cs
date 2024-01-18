using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTriggerDeleter
    {
        [FunctionName(nameof(StreamTriggerDeleter))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTriggerDeleter), pollingIntervalInMs: IntegrationTestHelpers.pollingIntervalShort)] StreamEntry entry,
            [Redis(IntegrationTestHelpers.connectionStringSetting, "XDEL")] out string arguments,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key '{nameof(StreamTriggerDeleter)}' with Id '{entry.Id}' and values '{JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString()))}");
            arguments = $"{nameof(StreamTriggerDeleter)} {entry.Id}";
        }
    }
}
