using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input binding that excutes a Redis command.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RedisScriptAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Redis connection string setting.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The script to be executed on the cache.
        /// </summary>
        [AutoResolve]
        public string Script { get; set; }

        /// <summary>
        /// Space-delimited keys for the script.
        /// </summary>
        [AutoResolve]
        public string Keys { get; set; }

        /// <summary>
        /// Space-delimited values for the script.
        /// </summary>
        [AutoResolve]
        public string Args { get; set; }
    }
}
