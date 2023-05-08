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
        public RedisPubSubTriggerAttribute(string connectionStringSetting, string channel)
        {
            ConnectionStringSetting = connectionStringSetting;
            Channel = channel;
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
    }
}