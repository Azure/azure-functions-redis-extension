using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Services
{
    /// <summary>
    /// Returns an <see cref="IConnectionMultiplexer"/> connected to the given connection string.
    /// </summary>
    public class CachedRedisConnectionMultiplexerService : IRedisConnectionMultiplexerService
    {
        private ConcurrentDictionary<string, Task<IConnectionMultiplexer>> connections = new ConcurrentDictionary<string, Task<IConnectionMultiplexer>>();

        /// <summary>
        /// Returns an <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task<IConnectionMultiplexer> ConnectAsync(string connectionString)
        {
            return await connections.GetOrAdd(connectionString, async (cs) => await ConnectionMultiplexer.ConnectAsync(cs));
        }
    }
}
