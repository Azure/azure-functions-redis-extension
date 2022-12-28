using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class RedisStreamsTriggerAttribute : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Array of keys to read from.
        /// </summary>
        public string Keys { get; set; }

        /// <summary>
        /// Gives the name of the consumer group that the function will use when reading from the stream.
        /// </summary>
        [AutoResolve]
        public string ConsumerGroup { get; set; } = null;

        /// <summary>
        /// The consumer name that the function will use when reading from the stream.
        /// </summary>
        [AutoResolve]
        public string ConsumerName { get; set; } = null;

        /// <summary>
        /// Number of elements to read from the stream at a time.
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// How often to poll the stream in ms.
        /// </summary>
        public int PollingInterval { get; set; } = 5000;
    }
}