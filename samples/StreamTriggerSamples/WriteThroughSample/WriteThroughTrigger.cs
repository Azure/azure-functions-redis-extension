using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class WriteThroughTrigger
    {
        public const string localhostSetting = "redisConnectionString";

        // Write through
        [FunctionName(nameof(WriteThrough))]
        public static void WriteThrough(
                [RedisStreamTrigger(localhostSetting, "stream1")] StreamEntry entry,
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


public class Data
{
    public string id { get; set; }
    public Dictionary<string, string> values { get; set; }
}
