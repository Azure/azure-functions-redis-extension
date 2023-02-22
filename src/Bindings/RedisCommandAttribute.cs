using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input binding that excutes a command on the redis instances and returns the reult.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RedisCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }

        /// <summary>
        /// The command to be executed on the cache.
        /// </summary>
        [AutoResolve]
        public string Command { get; set; }

        /// <summary>
        /// Arguments for the <see cref="Command"/>.
        /// </summary>
        [AutoResolve]
        public string Args { get; set; }
    }
}
