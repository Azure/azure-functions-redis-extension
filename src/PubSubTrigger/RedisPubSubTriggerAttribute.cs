using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Triggers on a PubSub channel, keyspace notification, or keyevent notification.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisPubSubTriggerAttribute : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        [ConnectionString]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The channel that the trigger will listen to.
        /// </summary>
        [AutoResolve]
        public string Channel { get; set; }
    }
}