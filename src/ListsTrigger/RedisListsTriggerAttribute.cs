using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis lists trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisListsTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisListsTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="keys">Keys to read from, space-delimited.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in milliseconds. Default: 1000</param>
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle. Default: 100</param>
        /// <param name="batchSize">Number of elements to pull from a Redis list at one time. Default: 10</param>
        /// <param name="listPopFromBeginning">Decides if the function will pop elements from the front or end of the list. Default: true</param>
        public RedisListsTriggerAttribute(string connectionStringSetting, string keys, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int batchSize = 10, bool listPopFromBeginning = true)
            : base(connectionStringSetting, keys, pollingIntervalInMs, messagesPerWorker, batchSize)
        {
            ListPopFromBeginning = listPopFromBeginning;
        }

        /// <summary>
        /// Decides if the function will pop elements from the front or end of the list.
        /// True (default) = pop elements from the front of the list.
        /// False = pop elements from the end of the list.
        /// </summary>
        public bool ListPopFromBeginning { get; }
    }
}