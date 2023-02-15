using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class DefaultRedisService : IRedisService
    {
        private readonly string connectionString;
        private readonly ILogger logger;

        private static IConnectionMultiplexer multiplexer;
        private static int count;
        private static object lockObj;

        public DefaultRedisService(string connectionString, ILogger logger)
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

        public async void Subscribe(string channel, Func<ChannelMessage, Task> handler)
        {
            logger?.LogInformation($"Subscribing to Redis pubsub channel '{channel}'");
            ChannelMessageQueue channelMessageQeueue = await multiplexer.GetSubscriber().SubscribeAsync(channel);
            channelMessageQeueue.OnMessage(handler);
        }
        
        public void Close()
        {
            lock (lockObj)
            {
                if (--count == 0)
                {
                    multiplexer.Close();
                    multiplexer.Dispose();
                    multiplexer = null;
                }
            }
        }
    }
}
