namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// This model gets returned when the function is triggered.
    /// </summary>
    public class RedisMessageModel
    {
        /// <summary>
        /// The Redis data type that the function was triggered on.
        /// <seealso cref="RedisTriggerType"/>
        /// </summary>
        public RedisTriggerType TriggerType { get; set; }

        /// <summary>
        /// The pubsub channel, key, event, stream key, or list key that the function was triggered on.
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// The message from the pubsub channel, keyspace notification, stream, or list that the function was triggered on.
        /// </summary>
        public string Message { get; set; }
    }
}
