using StackExchange.Redis;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRedisConnectionMultiplexerService
    {
        /// <summary>
        /// Returns an <see cref="IConnectionMultiplexer"/> connected to the Redis instance using the given connection string.
        /// </summary>
        /// <param name="connectionString">Redis connection string</param>
        /// <returns></returns>
        Task<IConnectionMultiplexer> ConnectAsync(string connectionString);
    }
}
