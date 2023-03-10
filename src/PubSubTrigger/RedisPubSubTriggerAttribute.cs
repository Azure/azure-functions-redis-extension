using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis pubsub trigger binding attributes.
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
        /// This setting will be used to resolve the actual connection string from the appsettings.
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