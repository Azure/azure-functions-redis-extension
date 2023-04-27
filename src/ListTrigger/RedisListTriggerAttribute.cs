using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis lists trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisListTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisListTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in milliseconds. Default: 1000</param>
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle. Default: 100</param>
        /// <param name="count">Number of elements to pull from a Redis list at one time. Default: 10</param>
        /// <param name="listPopFromBeginning">Decides if the function will pop elements from the front or end of the list. Default: true</param>
        public RedisListTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int count = 10, bool listPopFromBeginning = true)
            : base(connectionStringSetting, key, pollingIntervalInMs, messagesPerWorker, count)
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