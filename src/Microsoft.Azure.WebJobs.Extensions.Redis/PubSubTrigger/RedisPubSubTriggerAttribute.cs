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
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="channel">Redis pubsub channel name.</param>
        /// <param name="pattern">If the given channel is a pattern. Default: false</param>
        public RedisPubSubTriggerAttribute(string connection, string channel, bool pattern = false)
        {
            Connection = connection;
            Channel = channel;
            Pattern = pattern;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        [ConnectionString]
        public string Connection { get; }

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