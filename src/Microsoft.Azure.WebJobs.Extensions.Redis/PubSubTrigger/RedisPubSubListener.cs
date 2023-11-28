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
        internal IConnectionMultiplexer multiplexer;
        internal string channel;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;
        internal string logPrefix;

        public RedisPubSubListener(string name, IConnectionMultiplexer multiplexer, string channel, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.multiplexer = multiplexer;
            this.channel = channel;
            this.executor = executor;
            this.logger = logger;
            this.logPrefix = $"[Name:{name}][Trigger:RedisPubSubTrigger][Channel:{channel}]";
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ChannelMessageQueue channelMessageQeueue = await multiplexer.GetSubscriber().SubscribeAsync(channel);
            channelMessageQeueue.OnMessage(async (message) =>
            {
                logger?.LogDebug($"{logPrefix} Message received on channel '{channel}'.");
                await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = message }, cancellationToken);
            });
            logger?.LogInformation($"{logPrefix} Subscribed to channel '{channel}'.");
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
