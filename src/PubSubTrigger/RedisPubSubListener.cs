using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Redis instance.
    /// </summary>
    internal sealed class RedisPubSubListener : IListener
    {
        internal string connectionString;
        internal string channel;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;

        internal IConnectionMultiplexer multiplexer;

        public RedisPubSubListener(string connectionString, string channel, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.connectionString = connectionString;
            this.channel = channel;
            this.executor = executor;
            this.logger = logger;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (multiplexer is null)
            {
                logger?.LogInformation($"[{nameof(RedisPubSubListener)}] Connecting to Redis.");
                multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }

            if (!multiplexer.IsConnected)
            {
                logger?.LogCritical($"[{nameof(RedisPubSubListener)}] Failed to connect to cache.");
                throw new ArgumentException("Failed to connect to cache.");
            }

            ChannelMessageQueue channelMessageQeueue = await multiplexer.GetSubscriber().SubscribeAsync(channel);
            channelMessageQeueue.OnMessage(async (msg) =>
            {
                logger?.LogDebug($"[{nameof(RedisPubSubListener)}] Message received on channel '{channel}'.");
                var callBack = new RedisMessageModel
                {
                    Trigger = msg.Channel,
                    Message = msg.Message
                };

                await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = callBack }, cancellationToken);
            });
            logger?.LogInformation($"[{nameof(RedisPubSubListener)}] Subscribed to channel '{channel}'.");

            return;
        }

        /// <summary>
        /// Triggers disconnect from cache when cancellation token is invoked.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CloseMultiplexerAsync(multiplexer);
        }

        public async void Cancel()
        {
            await CloseMultiplexerAsync(multiplexer);
        }

        public async void Dispose()
        {
            await CloseMultiplexerAsync(multiplexer);
        }

        /// <summary>
        /// Closes redis cache multiplexer connection.
        /// </summary>
        internal async Task CloseMultiplexerAsync(IConnectionMultiplexer existingMultiplexer)
        {
            logger?.LogInformation($"[{nameof(RedisPubSubListener)}] Closing multiplexer.");
            await existingMultiplexer.CloseAsync();
            await existingMultiplexer.DisposeAsync();
        }
    }
}
