using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class RedisStreamsTriggerAttribute : RedisPollingTriggerAttributeBase
    {
        /// <summary>
        /// Gives the name of the consumer group that the function will use when reading from the stream.
        /// </summary>
        public string StreamConsumerGroup { get; set; } = null;

        /// <summary>
        /// The consumer name that the function will use when reading from the stream.
        /// </summary>
        public string StreamConsumerName { get; set; } = null;
    }
}