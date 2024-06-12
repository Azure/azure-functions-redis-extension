using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models
{
    public class StreamData
    {
        public string id { get; set; }
        public Dictionary<string, string> values { get; set; }

        // Helper method to format stream message
        public static StreamData Format(StreamEntry entry, ILogger logger)
        {
            logger.LogInformation("ID: {val}", entry.Id.ToString());

            // Map each key value pair
            Dictionary<string, string> dict = entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString());

            StreamData sampleItem = new StreamData { id = entry.Id, values = dict };
            return sampleItem;
        }
    }
}
