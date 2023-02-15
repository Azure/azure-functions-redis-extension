using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Services
{
    /// <summary>
    /// Returns a new IConnectionMultiplexer each time the constructor is called.
    /// </summary>
    public class BasicRedisConnectionMultiplexerService : IRedisConnectionMultiplexerService
    {
        /// <summary>
        /// Returns a new <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<IConnectionMultiplexer> ConnectAsync(string connectionString)
        {
            return await ConnectionMultiplexer.ConnectAsync(connectionString);
        }
    }
}
