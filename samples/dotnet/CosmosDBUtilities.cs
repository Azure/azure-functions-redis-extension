using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public class CosmosDBData
    {
        public string id { get; set; }
        public Dictionary<string, string> values { get; set; }

        // Helper method to format stream message
        public static CosmosDBData Format(StreamEntry entry, ILogger logger)
        {
            logger.LogInformation("ID: {val}", entry.Id.ToString());

            // Map each key value pair
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            CosmosDBData sampleItem = new CosmosDBData { id = entry.Id, values = dict };
            return sampleItem;
        }
    }
}


