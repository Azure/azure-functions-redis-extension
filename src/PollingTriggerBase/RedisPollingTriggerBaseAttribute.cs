using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Base attributes for all triggers that poll Redis.
    /// </summary>
    public abstract class RedisPollingTriggerBaseAttribute : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        [ConnectionString]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Keys to read from, space-delimited.
        /// </summary>
        [AutoResolve]
        public string Keys { get; set; }

        /// <summary>
        /// How often to poll Redis in milliseconds.
        /// </summary>
        public int PollingIntervalInMs { get; set; } = 1000;

        /// <summary>
        /// Used to determine how many workers the function should scale to.
        /// For example, if the number of <see cref="MessagesPerWorker">MessagesPerWorker</see> is 10,
        /// and there are 1500 elements remaining in the list,
        /// the functions host will attempt to scale up to 150 instances.
        /// </summary>
        public int MessagesPerWorker { get; set; } = 1000;

        /// <summary>
        /// Number of elements to pull from Redis at one time.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}