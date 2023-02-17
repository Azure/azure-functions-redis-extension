using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Attribute used to bind parameters to a Redis list trigger message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class RedisListsTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisListsTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="keys">Keys to read from, space-delimited.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle.</param>
        /// <param name="batchSize">Number of elements to pull from Redis at one time.</param>
        /// <param name="listPopFromBeginning">Decides if the function will pop elements from the front or end of the list.</param>
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