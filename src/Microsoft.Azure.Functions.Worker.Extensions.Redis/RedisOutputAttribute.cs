using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// An output binding that excutes a command on the redis instances.
    /// </summary>
    public sealed class RedisOutputAttribute : OutputBindingAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisOutputAttribute"/>.
        /// </summary>
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisOutputAttribute(string connection, string command)
        {
            Connection = connection;
            Command = command;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        public string Connection { get; }

        /// <summary>
        /// The command to be executed on the cache without any arguments.
        /// </summary>
        public string Command { get; }
    }
}