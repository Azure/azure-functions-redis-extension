using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Base attributes for all triggers that poll Redis.
    /// </summary>
    public abstract class RedisPollingTriggerAttributeBase : Attribute
    {
        /// <summary>
        /// Cache connection string.
        /// </summary>
        [ConnectionString]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Keys to read from, space-delimited.
        /// All keys will be read from at the same time, using the <a href="https://redis.io/commands/xread/">XREAD</a> or <a href="https://redis.io/commands/lmpop/">LMPOP</a> commands.
        /// </summary>
        [AutoResolve]
        public string Keys { get; set; }

        /// <summary>
        /// How often to poll Redis.
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Used to determine how many workers the function should scale to.
        /// For example, if the number of <see cref="MessagesPerWorker">MessagesPerWorker</see> is 10,
        /// and there are 1500 elements remaining in the list,
        /// the functions host will attempt to scale up to 150 instances.
        /// </summary>
        public int MessagesPerWorker { get; set; } = 1000;

        /// <summary>
        /// Number of elements to pull from Redis at one time.
        /// This is done using the COUNT argument in <a href="https://redis.io/commands/xread/">XREAD</a> for streams and <a href="https://redis.io/commands/lpop/">LPOP</a> for lists.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}