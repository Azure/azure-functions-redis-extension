using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{

    /// <summary>
    /// Triggers on a PubSub channel, keyspace notification, or keyevent notification.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
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
        [AutoResolve]
        public string TriggerType { get; set; } = "PubSub";

        /// <summary>
        /// The pubsub channel, key, or event that the function will trigger on.
        /// </summary>
        [AutoResolve]
        public string Trigger { get; set; }
    }
}