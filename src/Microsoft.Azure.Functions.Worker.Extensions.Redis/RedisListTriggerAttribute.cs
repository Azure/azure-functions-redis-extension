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
        /// <param name="messagesPerWorker">The number of messages each functions instance is expected to handle.</param>
        /// <param name="count">Number of elements to pull from Redis at one time.</param>
        /// <param name="listPopFromBeginning">Decides if the function will pop elements from the front or end of the list. Default: true</param>
        public RedisListTriggerAttribute(string connectionStringSetting, string key, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int count = 10, bool listPopFromBeginning = true)
        {
            ConnectionStringSetting = connectionStringSetting;
            Key = key;
            PollingIntervalInMs = pollingIntervalInMs;
            MessagesPerWorker = messagesPerWorker;
            Count = count;
            ListPopFromBeginning = listPopFromBeginning;
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
        /// Decides if the function will pop elements from the front or end of the list.
        /// True (default) = pop elements from the front of the list.
        /// False = pop elements from the end of the list.
        /// </summary>
        public bool ListPopFromBeginning { get; }
    }
}