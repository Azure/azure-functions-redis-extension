using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// An input binding that executes a command on Redis instance and returns the value to the function.
    /// </summary>
    public sealed class RedisInputAttribute : InputBindingAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisInputAttribute"/>.
        /// </summary>
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisInputAttribute(string connection, string command)
        {
            Connection = connection;
            Command = command;
        }

        /// <summary>
        /// App setting name that contains Redis connection information.
        /// </summary>
        public string Connection { get; }

        /// <summary>
        /// The full command to be executed on the cache, with space-delimited arguments.
        /// </summary>
        public string Command { get; }
    }
}