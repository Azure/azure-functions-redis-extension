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
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisInputAttribute(string connectionStringSetting, string command)
        {
            ConnectionStringSetting = connectionStringSetting;
            Command = command;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The command to be executed on the cache.
        /// </summary>
        public string Command { get; }
    }
}