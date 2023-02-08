namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// The different types of notifications that the function will trigger on.
    /// </summary>
    public enum RedisTriggerType
    {
        /// <summary>
        /// Triggers on <a href="https://redis.io/docs/manual/pubsub/">Redis Pub/Sub</a> channels.
        /// </summary>
        PubSub,

        /// <summary>
        /// Triggers on <a href="https://redis.io/docs/manual/keyspace-notifications/">Redis keyspace notifications</a> using the <see cref="RedisPubSubTriggerAttribute">RedisPubSubTrigger</see>.
        /// These notifications must be enabled on the Redis cache.
        /// </summary>
        KeySpace,

        /// <summary>
        /// Triggers on <a href="https://redis.io/docs/manual/keyspace-notifications/">Redis keyevent notifications</a> using the <see cref="RedisPubSubTriggerAttribute">RedisPubSubTrigger</see>
        /// </summary>
        KeyEvent,

        /// <summary>
        /// Triggers on elements in <a href="https://redis.io/docs/data-types/streams/">Redis streams</a>.
        /// </summary>
        Stream,

        /// <summary>
        /// Triggers on elements in <a href="https://redis.io/docs/data-types/lists/">Redis lists</a>.
        /// </summary>
        List
    }
}