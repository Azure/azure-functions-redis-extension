using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// 
    /// </summary>
    internal class CountingRedisService : IRedisService, IDisposable
    {
        private readonly string connectionString;
        private readonly ILogger logger;

        private static IConnectionMultiplexer multiplexer;
        private static int count;
        private static object lockObj;

        public CountingRedisService(string connectionString, ILogger logger)
        {
            this.connectionString = connectionString;
            this.logger = logger;
            count = 0;
            lockObj = new object();
        }

        public void Connect()
        {
            lock(lockObj)
            {
                if (multiplexer is null)
                {
                    logger?.LogInformation("Connecting to Redis.");
                    multiplexer = ConnectionMultiplexer.Connect(connectionString);
                    multiplexer.GetDatabase().StringIncrement("AzureFunctionConnections");
                }
                else if (!multiplexer.IsConnected)
                {
                    logger?.LogCritical("Failed to connect to Redis.");
                    throw new ArgumentException("Failed to connect to Redis.");
                }
                else
                {
                    logger?.LogInformation("Already connected to Redis.");
                }
                count++;
            }
        }

        // multiplexer functions used by the triggers
        public Version GetServerVersion() => multiplexer.GetServers()[0].Version;
        public void Subscribe(RedisChannel channel, Func<ChannelMessage, Task> handler) => multiplexer.GetSubscriber().Subscribe(channel).OnMessage(handler);
        public async Task<RedisValue> ListLeftPopAsync(RedisKey key) => await multiplexer.GetDatabase().ListLeftPopAsync(key);
        public async Task<RedisValue[]> ListLeftPopAsync(RedisKey key, long count) => await multiplexer.GetDatabase().ListLeftPopAsync(key, count);
        public async Task<ListPopResult> ListLeftPopAsync(RedisKey[] keys, long count) => await multiplexer.GetDatabase().ListLeftPopAsync(keys, count);
        public async Task<RedisValue> ListRightPopAsync(RedisKey key) => await multiplexer.GetDatabase().ListRightPopAsync(key);
        public async Task<RedisValue[]> ListRightPopAsync(RedisKey key, long count) => await multiplexer.GetDatabase().ListRightPopAsync(key, count);
        public async Task<ListPopResult> ListRightPopAsync(RedisKey[] keys, long count) => await multiplexer.GetDatabase().ListRightPopAsync(keys, count);
        public async Task<long> ListLengthAsync(RedisKey key) => await multiplexer.GetDatabase().ListLengthAsync(key);


        public void Close()
        {
            lock (lockObj)
            {
                if (--count == 0)
                {
                    CloseInternal();
                }
            }
        }

        private void CloseInternal()
        {
            if (!(multiplexer is null))
            {
                multiplexer.Close();
                multiplexer.Dispose();
                multiplexer = null;
            }
        }

        public void Dispose()
        {
            multiplexer.Close();
        }
    }
}
