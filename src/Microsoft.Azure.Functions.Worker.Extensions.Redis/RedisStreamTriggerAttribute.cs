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
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in ms.</param>
        /// <param name="maxBatchSize">Number of entries to pull from Redis at one time.</param>
        /// <param name="deleteAfterProcess">Decides if the function will delete the stream entries after processing. Default: false</param>
        public RedisStreamTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16, bool deleteAfterProcess = false)
        {
            ConnectionStringSetting = connectionStringSetting;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MaxBatchSize = maxBatchSize;
            DeleteAfterProcess = deleteAfterProcess;
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
        /// Decides if the function will delete the stream entries after processing.
        /// </summary>
        public bool DeleteAfterProcess { get; }
    }
}