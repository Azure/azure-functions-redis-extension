using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisOutputBindingTestFunctions
    {
        public const string keyeventChannelSet = "__keyevent@0__:set";
        public const int pollingInterval = 100;

        [FunctionName(nameof(SetDeleter))]
        public static void SetDeleter(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyeventChannelSet)] string key,
            [Redis(IntegrationTestHelpers.connectionStringSetting, "DEL")] out string[] arguments,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            arguments = new string[] { key };
        }

        [FunctionName(nameof(StreamTriggerDeleter))]
        public static void StreamTriggerDeleter(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTriggerDeleter), pollingIntervalInMs: pollingInterval)] StreamEntry entry,
            [Redis(IntegrationTestHelpers.connectionStringSetting, "XDEL")] out string[] arguments,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key '{nameof(StreamTriggerDeleter)}' with Id '{entry.Id}' and values '{JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString()))}");
            arguments = new string[] { nameof(StreamTriggerDeleter), entry.Id.ToString() };
        }


        [FunctionName(nameof(MultipleAddAsyncCalls))]
        public static void MultipleAddAsyncCalls(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(MultipleAddAsyncCalls))] string entry,
            [Redis(IntegrationTestHelpers.connectionStringSetting, "SET")] IAsyncCollector<string[]> collector,
            ILogger logger)
        {
            string[] keys = entry.Split(',');
            foreach (string key in keys)
            {
                collector.AddAsync(new string[] { key, nameof(MultipleAddAsyncCalls) }).Wait();
            }
        }
    }
}
