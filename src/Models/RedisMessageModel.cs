namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// This model gets returned when the function is triggered.
    /// </summary>
    public class RedisMessageModel
    {
        /// <summary>
        /// The pubsub channel, stream key, or list key that the function was triggered on.
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// The message from the pubsub channel, stream, or list that the function was triggered on.
        /// </summary>
        public string Message { get; set; }
    }
}
