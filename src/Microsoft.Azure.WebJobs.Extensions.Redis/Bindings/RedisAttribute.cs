﻿using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An output binding that excutes a command on the redis instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class RedisAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisAttribute(string connectionStringSetting, string command)
        {
            ConnectionStringSetting = connectionStringSetting;
            Command = command;
        }

        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        [AutoResolve]
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The command to be executed on the cache.
        /// </summary>
        [AutoResolve]
        public string Command { get; }
    }
}