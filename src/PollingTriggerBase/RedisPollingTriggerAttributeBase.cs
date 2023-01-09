using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    public abstract class RedisPollingTriggerAttributeBase : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// How often to poll redis in milliseconds.
        /// </summary>
        public int PollingInterval { get; set; } = 5000;

        /// <summary>
        /// How many messages should be assigned to a worker at a time.
        /// Used to determine how many workers the function should scale to.
        /// </summary>
        public int MessagesPerWorker { get; set; } = 1000;

        /// <summary>
        /// Keys to read from, space-delimited.
        /// </summary>
        public string Keys { get; set; }

        /// <summary>
        /// Number of elements to pull from redis at one time.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}