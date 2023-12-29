using System;
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
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        [AutoResolve]
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// For an input binding, this is the redis command with space-delimited arguments.
        /// For an output binding, this is redis command without any arguments.
        /// </summary>
        [AutoResolve]
        public string Command { get; }
    }
}