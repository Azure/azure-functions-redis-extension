using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models
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
            Dictionary<string, string> dict = StreamEntryToDictionary(entry);

            CosmosDBData sampleItem = new CosmosDBData { id = entry.Id, values = dict };
            return sampleItem;
        }

        internal static Dictionary<string, string> StreamEntryToDictionary(StreamEntry entry)
        {
            return entry.Values.ToDictionary((NameValueEntry value) => value.Name.ToString(), (NameValueEntry value) => value.Value.ToString());
        }
    }

    public record RedisData(
        string id,
        string key,
        string value,
        DateTime timestamp
        );

    public record PubSubData(
        string id,
        string channel,
        string message,
        DateTime timestamp
        );

    public record CosmosDBListData(
        string id,
        List<string> value
        );

}
