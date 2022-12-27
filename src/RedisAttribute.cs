using System;
using System.Data;
using Microsoft.Azure.WebJobs.Description;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// An input and output binding that can be used to establish a connection to a Redis instance to get or set data.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class RedisAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionString { get; set; }
    }
}
