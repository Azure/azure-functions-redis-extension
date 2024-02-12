using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// Redis pubsub trigger binding attributes.
    /// </summary>
    public sealed class RedisPubSubTriggerAttribute : TriggerBindingAttribute
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
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The channel that the trigger will listen to.
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// If the given channel is a pattern.
        /// </summary>
        public bool Pattern { get; }
    }
}