using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input binding that excutes a command on the redis instances and returns the reult.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class RedisCommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisCommandAttribute"/>.
        /// </summary>
        /// <param name="connectionStringSetting"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public RedisCommandAttribute(string connectionStringSetting, string command, string args) {
            ConnectionStringSetting = connectionStringSetting;
            Command = command;
            Args = args;
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

        /// <summary>
        /// Arguments for the command.
        /// </summary>
        [AutoResolve]
        public string Args { get; }
    }
}
