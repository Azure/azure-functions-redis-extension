using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisStreamTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisStreamTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in milliseconds. Default: 1000</param>
        /// <param name="maxBatchSize">Number of entries to pull from a Redis stream at one time. Default: 16</param>
        public RedisStreamTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16)
            : base(connectionStringSetting, key, pollingIntervalInMs, maxBatchSize)
        {
        }
    }
}