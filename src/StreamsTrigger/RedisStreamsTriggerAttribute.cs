using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisStreamsTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisStreamsTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="keys">Keys to read from, space-delimited.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in milliseconds. Default: 1000</param>
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle. Default: 100</param>
        /// <param name="batchSize">Number of elements to pull from a Redis stream at one time. Default: 10</param>
        /// <param name="consumerGroup">Name of the consumer group to use when reading the streams. Default: AzureFunctionRedisExtension</param>
        /// <param name="deleteAfterProcess">Decides if the function will delete the stream entries after processing. Default: false</param>
        public RedisStreamsTriggerAttribute(string connectionStringSetting, string keys, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int batchSize = 10, string consumerGroup = "AzureFunctionRedisExtension", bool deleteAfterProcess = false)
            : base(connectionStringSetting, keys, pollingIntervalInMs, messagesPerWorker, batchSize)
        {
            ConsumerGroup = consumerGroup;
            DeleteAfterProcess = deleteAfterProcess;
        }

        /// <summary>
        /// Name of the consumer group to use when reading the streams.
        /// </summary>
        public string ConsumerGroup { get; }

        /// <summary>
        /// Decides if the function will delete the stream entries after processing.
        /// </summary>
        public bool DeleteAfterProcess { get; }

    }
}