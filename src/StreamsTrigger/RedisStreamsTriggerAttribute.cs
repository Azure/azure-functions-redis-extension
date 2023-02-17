using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisStreamsTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionStringSetting"></param>
        /// <param name="keys"></param>
        /// <param name="pollingIntervalInMs"></param>
        /// <param name="messagesPerWorker"></param>
        /// <param name="batchSize"></param>
        /// <param name="consumerGroup"></param>
        /// <param name="deleteAfterProcess"></param>
        public RedisStreamsTriggerAttribute(string connectionStringSetting, string keys, int pollingIntervalInMs = 1000, int messagesPerWorker = 100, int batchSize = 10, string consumerGroup = "AzureFunctionRedisExtension", bool deleteAfterProcess = false)
            : base(connectionStringSetting, keys, pollingIntervalInMs, messagesPerWorker, batchSize)
        {
            ConsumerGroup = consumerGroup;
            DeleteAfterProcess = deleteAfterProcess;
        }

        /// <summary>
        /// Name of the consumer group to use when reading the streams.
        /// </summary>
        public string ConsumerGroup { get; }

        /// <summary>
        /// If true, the function will delete the stream entries after processing.
        /// </summary>
        public bool DeleteAfterProcess { get; }

    }
}