using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis
{
    /// <summary>
    /// An output binding that excutes a command on the redis instances.
    /// </summary>
    public sealed class RedisAttribute : BindingAttribute
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
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The command to be executed on the cache.
        /// </summary>
        public string Command { get; }
    }
}