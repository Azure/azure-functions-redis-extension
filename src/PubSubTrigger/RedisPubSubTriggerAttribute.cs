﻿using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]

    /// <summary>
    /// Trigger binding attributes.
    /// </summary>
    public class RedisPubSubTriggerAttribute : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The type of notification that the function will trigger on.
        /// </summary>
        public RedisTriggerType TriggerType { get; set; } = RedisTriggerType.PubSub;

        /// <summary>
        /// The pubsub channel, key, or event that the function will trigger on.
        /// </summary>
        [AutoResolve]
        public string Trigger { get; set; }
    }
}