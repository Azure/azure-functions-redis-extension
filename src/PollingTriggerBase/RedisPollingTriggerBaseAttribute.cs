using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Base attributes for all triggers that poll Redis.
    /// </summary>
    public abstract class RedisPollingTriggerBaseAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisPollingTriggerBaseAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="keys">Keys to read from, space-delimited.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle.</param>
        /// <param name="batchSize">Number of elements to pull from Redis at one time.</param>
        public RedisPollingTriggerBaseAttribute(string connectionStringSetting, string keys, int pollingIntervalInMs, int messagesPerWorker, int batchSize)
        {
            this.ConnectionStringSetting = connectionStringSetting;
            this.Keys = keys;
            this.PollingIntervalInMs = pollingIntervalInMs;
            this.MessagesPerWorker = messagesPerWorker;
            this.BatchSize = batchSize;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        [ConnectionString]
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// Keys to read from, space-delimited.
        /// </summary>
        [AutoResolve]
        public string Keys { get; }

        /// <summary>
        /// How often to poll Redis in milliseconds.
        /// </summary>
        public int PollingIntervalInMs { get; }

        /// <summary>
        /// The number of messages each functions instance is expected to handle.
        /// Used to determine how many workers the function should scale to.
        /// For example, if the number of <see cref="MessagesPerWorker">MessagesPerWorker</see> is 10,
        /// and there are 1500 elements remaining in the list,
        /// the functions host will attempt to scale up to 150 instances.
        /// </summary>
        public int MessagesPerWorker { get; }

        /// <summary>
        /// Number of elements to pull from Redis at one time.
        /// </summary>
        public int BatchSize { get; }
    }
}