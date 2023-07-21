using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const string localhostSetting = "redisConnectionString";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(WriteThroughTest))]
        public static void WriteThroughTest(
            [RedisStreamTrigger(localhostSetting, nameof(WriteThroughTest), pollingIntervalInMs: pollingInterval)] StreamEntry entry,
            [CosmosDB(
                databaseName: "database-id",
                containerName: "container-id",
                Connection = "cosmosConnectionString")]
                ICollector<Data> items,
            ILogger logger)
        {
            Data item = FormatData(entry, logger);

            // Insert data into CosmosDB synchronously
            items.Add(item);
        }

        private static Data FormatData(StreamEntry entry, ILogger logger)
        {
            // Map each key value pair
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            Data sampleItem = new Data { id = entry.Id, values = dict };
            return sampleItem;
        }
    }
}
