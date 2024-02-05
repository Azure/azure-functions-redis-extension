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
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time.</param>
        /// <param name="listDirection">The direction to pop elements from the list. Default: left</param>
        public RedisListTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16, ListDirection listDirection = ListDirection.LEFT)
        {
            ConnectionStringSetting = connectionStringSetting;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MaxBatchSize = maxBatchSize;
            ListDirection = listDirection;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        public string ConnectionStringSetting { get; }

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
        /// Uses <a href="https://redis.io/commands/lmpop/">LPOP</a> or <a href="https://redis.io/commands/lmpop/">LMPOP RIGHT</a>
        /// </summary>
        LEFT,

        /// <summary>
        /// Uses <a href="https://redis.io/commands/rpop/">RPOP</a> or <a href="https://redis.io/commands/lmpop/">LMPOP RIGHT</a>
        /// </summary>
        RIGHT
    }
}