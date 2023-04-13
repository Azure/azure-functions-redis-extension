using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        internal string logPrefix;

        internal IConnectionMultiplexer multiplexer;

        public RedisPubSubListener(string id, string connectionString, string channel, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.connectionString = connectionString;
            this.channel = channel;
            this.executor = executor;
            this.logger = logger;
            this.logPrefix = $"[RedisPubSubTrigger][Id:{id}]";
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (multiplexer is null)
            {
                logger?.LogInformation($"{logPrefix} Connecting to Redis.");
                multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }

            if (!multiplexer.IsConnected)
            {
                logger?.LogCritical($"{logPrefix} Failed to connect to cache.");
                throw new ArgumentException("Failed to connect to cache.");
            }

            ChannelMessageQueue channelMessageQeueue = await multiplexer.GetSubscriber().SubscribeAsync(channel);
            channelMessageQeueue.OnMessage(async (msg) =>
            {
                logger?.LogDebug($"{logPrefix} Message received on channel '{channel}'.");
                var triggerValue = new RedisPubSubMessage(msg.SubscriptionChannel, msg.Channel, msg.Message);
                await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
            });
            logger?.LogInformation($"{logPrefix} Subscribed to channel '{channel}'.");

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
            logger?.LogInformation($"{logPrefix} Closing and disposing multiplexer.");
            await existingMultiplexer.CloseAsync();
            await existingMultiplexer.DisposeAsync();
        }
    }
}
