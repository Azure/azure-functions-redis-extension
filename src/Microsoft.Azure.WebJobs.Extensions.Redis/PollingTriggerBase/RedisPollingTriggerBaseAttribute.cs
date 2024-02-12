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
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time. Used to determine scaling.</param>
        public RedisPollingTriggerBaseAttribute(string connection, string key, int pollingIntervalInMs, int maxBatchSize)
        {
            this.Connection = connection;
            this.Key = key;
            this.PollingIntervalInMs = pollingIntervalInMs;
            this.MaxBatchSize = maxBatchSize;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        [ConnectionString]
        public string Connection { get; }

        /// <summary>
        /// Key to read from.
        /// </summary>
        [AutoResolve]
        public string Key { get; }

        /// <summary>
        /// How often to poll Redis in milliseconds.
        /// </summary>
        public int PollingIntervalInMs { get; }

        /// <summary>
        /// Number of entries to pull from Redis at one time.
        /// </summary>
        public int MaxBatchSize { get; }
    }
}