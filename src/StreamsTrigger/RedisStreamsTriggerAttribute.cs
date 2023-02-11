﻿using System;
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
        /// Name of the consumer group to use when reading the streams.
        /// </summary>
        public string ConsumerGroup { get; set; } = "AzureFunctionRedisExtension";

        /// <summary>
        /// If true, the function will delete the stream entries after processing.
        /// </summary>
        public string DeleteAfterProcess { get; set; } = "false";

    }
}