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
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time. Used to determine scaling.</param>
        public RedisPollingTriggerBaseAttribute(string connectionStringSetting, string key, int pollingIntervalInMs, int maxBatchSize)
        {
            this.ConnectionStringSetting = connectionStringSetting;
            this.Key = key;
            this.PollingIntervalInMs = pollingIntervalInMs;
            this.MaxBatchSize = maxBatchSize;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        [ConnectionString]
        public string ConnectionStringSetting { get; }

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