namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis PubSub message.
    /// </summary>
    public class RedisPubSubMessage
    {
        /// <summary>
        /// Redis PubSub message.
        /// </summary>
        /// <param name="subscriptionChannel">The channel that the subscription was created from.</param>
        /// <param name="channel">The channel that the function was triggered on.</param>
        /// <param name="message">The message that was broadcast.</param>
        public RedisPubSubMessage(string subscriptionChannel, string channel, string message)
        {
            SubscriptionChannel = subscriptionChannel;
            Channel = channel;
            Message = message;
        }

        /// <summary>
        /// The channel that the subscription was created from.
        /// </summary>
        public string SubscriptionChannel { get; }

        /// <summary>
        /// The channel that the function was triggered on.
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// The message that was broadcast.
        /// </summary>
        public string Message { get; }
    }
}
