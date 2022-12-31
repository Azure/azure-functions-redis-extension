using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input binding that excutes a command on the redis instances and returns the reult.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class RedisScriptAttribute : Attribute
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
        public string LuaScript { get; set; }

        /// <summary>
        /// Space-delimited keys for the <see cref="RedisCommand"/>.
        /// </summary>
        [AutoResolve]
        public string Keys { get; set; }

        /// <summary>
        /// Space-delimited values for the <see cref="RedisCommand"/>.
        /// </summary>
        [AutoResolve]
        public string Values { get; set; }
    }
}
