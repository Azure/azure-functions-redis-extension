using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// Redis streams trigger binding attributes.
    /// </summary>
    public class RedisStreamTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisStreamTriggerAttribute"/>.
        /// </summary>
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time.</param>
        public RedisStreamTriggerAttribute(string connection, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16)
        {
            Connection = connection;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MaxBatchSize = maxBatchSize;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        public string Connection { get; }

        /// <summary>
        /// Key to read from.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// How often to poll Redis in milliseconds.
        /// </summary>
        public int PollingIntervalInMs { get; }

        /// <summary>
        /// Number of entries to pull from Redis at one time.
        /// </summary>
        public int MaxBatchSize { get; }
    }
}