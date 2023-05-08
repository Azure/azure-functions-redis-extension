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
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle.</param>
        /// <param name="count">Number of elements to pull from Redis at one time.</param>
        /// <param name="consumerGroup">Name of the consumer group to use when reading the streams. Default: AzureFunctionRedisExtension</param>
        /// <param name="deleteAfterProcess">Decides if the function will delete the stream entries after processing. Default: false</param>
        public RedisStreamTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int count = 10, string consumerGroup = "AzureFunctionRedisExtension", bool deleteAfterProcess = false)
        {
            ConnectionStringSetting = connectionStringSetting;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MessagesPerWorker = messagesPerWorker;
            Count = count;
            ConsumerGroup = consumerGroup;
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
        /// The number of messages each functions instance is expected to handle.
        /// Used to determine how many workers the function should scale to.
        /// For example, if the number of <see cref="MessagesPerWorker">MessagesPerWorker</see> is 10,
        /// and there are 1500 elements remaining in the list,
        /// the functions host will attempt to scale up to 150 instances.
        /// </summary>
        public int MessagesPerWorker { get; }

        /// <summary>
        /// Number of elements to pull from Redis at one time.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Name of the consumer group to use when reading the streams.
        /// </summary>
        public string ConsumerGroup { get; }

        /// <summary>
        /// Decides if the function will delete the stream entries after processing.
        /// </summary>
        public bool DeleteAfterProcess { get; }
    }
}