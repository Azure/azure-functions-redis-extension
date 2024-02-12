using Microsoft.Azure.WebJobs.Description;
using StackExchange.Redis;
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
        /// <param name="pattern">If the given channel is a pattern. Default: false</param>
        public RedisPubSubTriggerAttribute(string connectionStringSetting, string channel, bool pattern = false)
        {
            ConnectionStringSetting = connectionStringSetting;
            Channel = channel;
            Pattern = pattern;
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

        /// <summary>
        /// If the given channel is a pattern.
        /// </summary>
        [AutoResolve]
        public bool Pattern { get; }
    }
}