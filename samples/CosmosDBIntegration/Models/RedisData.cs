﻿using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models
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
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            StreamData sampleItem = new StreamData { id = entry.Id, values = dict };
            return sampleItem;
        }
    }

    public class StreamDataSingleDocument
    {
        public string id { get; set; }
        public int maxlen { get; set; }
        public Dictionary<string, Dictionary<string, string>> messages { get; set; }

        public static StreamDataSingleDocument CreateNewEntry(StreamEntry entry, string streamName, ILogger logger)
        {
            logger.LogInformation("Creating a new document for {val}. Inserting ID: {val} as the first entry", streamName, entry.Id.ToString());

            // Map each key value pair
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            // Create a new list of messages
            var list = new Dictionary<string, Dictionary<string, string>>();
            list.Add(entry.Id.ToString(), dict);

            StreamDataSingleDocument data = new StreamDataSingleDocument { id = streamName, maxlen = 1000, messages = list };
            return data;
        }

        public static StreamDataSingleDocument UpdateExistingEntry(StreamDataSingleDocument results, StreamEntry entry, ILogger logger)
        {
            logger.LogInformation("Adding to {val} document. Inserting ID: {val} ", results.id, entry.Id.ToString());

            // Map each key value pair
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            // Update list of messages
            var list = results.messages;
            list.Add(entry.Id.ToString(), dict);

            if (list.Count > results.maxlen)
            {
                string minKey = list.Keys.Min();
                list.Remove(minKey);
            }

            StreamDataSingleDocument data = new StreamDataSingleDocument { id = results.id, maxlen = results.maxlen, messages = list };
            return data;
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
