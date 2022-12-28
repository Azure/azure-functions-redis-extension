using System;
using System.Data;
using Microsoft.Azure.WebJobs.Description;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input and output binding that can be used to establish a connection to a Redis instance.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class RedisConnectionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }
    }
}
