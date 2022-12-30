using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger binding attributes
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
        public RedisTriggerType TriggerType { get; set; }

        /// <summary>
        /// The key, event, pubsub channel, or stream that the function will trigger on.
        /// </summary>
        [AutoResolve]
        public string Trigger { get; set; }
    }
}