using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// Redis list trigger binding attributes.
    /// </summary>
    public class RedisListTriggerAttribute : TriggerBindingAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisListTriggerAttribute"/>.
        /// </summary>
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time.</param>
        /// <param name="listDirection">The direction to pop elements from the list. Default: left</param>
        public RedisListTriggerAttribute(string connection, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16, ListDirection listDirection = ListDirection.LEFT)
        {
            Connection = connection;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MaxBatchSize = maxBatchSize;
            ListDirection = listDirection;
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

        /// <summary>
        /// The direction to pop elements from the list.
        /// </summary>
        public ListDirection ListDirection  { get; }
    }


    /// <summary>
    /// The direction to pop elements from the list.
    /// </summary>
    public enum ListDirection
    {
        /// <summary>
        /// Uses <a href="https://redis.io/commands/lpop/">LPOP</a>
        /// </summary>
        LEFT,

        /// <summary>
        /// Uses <a href="https://redis.io/commands/rpop/">RPOP</a>
        /// </summary>
        RIGHT
    }
}