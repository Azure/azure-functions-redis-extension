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
        /// <param name="connectionStringSetting">Redis connection string setting.</param>
        /// <param name="command">The command to be executed on the cache.</param>
        public RedisOutputAttribute(string connectionStringSetting, string command)
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
        /// The command to be executed on the cache without any arguments.
        /// </summary>
        public string Command { get; }
    }
}