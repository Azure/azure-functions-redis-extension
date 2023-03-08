namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// This model gets returned when the function is triggered.
    /// </summary>
    public class RedisTriggerModel
    {
        /// <summary>
        /// The pubsub channel, stream key, or list key that the function was triggered on.
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// The value from the pubsub channel, stream, or list that the function was triggered on.
        /// Pubsub and List values will be strings.
        /// Streams values will be a Dictionary of strings.
        /// </summary>
        public object Value { get; set; }
    }
}
