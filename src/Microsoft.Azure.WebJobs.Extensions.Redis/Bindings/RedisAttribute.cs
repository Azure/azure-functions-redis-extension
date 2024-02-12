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
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisAttribute(string connection, string command)
        {
            Connection = connection;
            Command = command;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        [AutoResolve]
        public string Connection { get; }

        /// <summary>
        /// For an input binding, this is the redis command with space-delimited arguments.
        /// For an output binding, this is the redis command without any arguments.
        /// </summary>
        [AutoResolve]
        public string Command { get; }
    }
}