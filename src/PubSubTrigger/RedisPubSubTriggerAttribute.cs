using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Attribute used to bind parameters to a Redis PubSub trigger message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class RedisPubSubTriggerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisPubSubTriggerAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="channel">Redis pubsub channel name.</param>
        public RedisPubSubTriggerAttribute(string connectionStringSetting, string channel)
        {
            ConnectionStringSetting = connectionStringSetting;
            Channel = channel;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the configuration.
        /// </summary>
        [ConnectionString]
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The channel that the trigger will listen to.
        /// </summary>
        [AutoResolve]
        public string Channel { get; }
    }
}