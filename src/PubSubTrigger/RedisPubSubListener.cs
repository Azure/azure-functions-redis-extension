using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Redis instance.
    /// </summary>
    internal sealed class RedisPubSubListener : IListener
    {
        internal const string KEYSPACE_TEMPLATE = "__keyspace@*__:{0}";
        internal const string KEYEVENT_TEMPLATE = "__keyevent@*__:{0}";

        internal ITriggeredFunctionExecutor executor;
        internal string connectionString;
        internal string trigger;
        internal RedisTriggerType triggerType;
        internal IConnectionMultiplexer multiplexer;

        public RedisPubSubListener(string connectionString, RedisTriggerType triggerType, string trigger, ITriggeredFunctionExecutor executor)
        {
            this.connectionString = connectionString;
            this.triggerType = triggerType;
            this.trigger = trigger;
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (multiplexer is null)
            {
                multiplexer = await InitializeConnectionMultiplexerAsync(connectionString);
            }

            if (!multiplexer.IsConnected)
            {
                throw new ArgumentException("Failed to connect to cache.");
            }

            switch (triggerType)
            {
                case RedisTriggerType.PubSub:
                    await EnablePubSubAsync(multiplexer, cancellationToken);
                    break;
                case RedisTriggerType.KeySpace:
                    await EnableKeySpaceAsync(multiplexer, cancellationToken);
                    break;
                case RedisTriggerType.KeyEvent:
                    await EnableKeyEventAsync(multiplexer, cancellationToken);
                    break;
                default:
                    throw new ArgumentException("RedisPubSubTrigger only supportsPubSub, KeySpace, and KeyEvent trigger types.");
            }
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
        /// Creates redis cache multiplexer connection.
        /// </summary>
        private static async Task<IConnectionMultiplexer> InitializeConnectionMultiplexerAsync(string connectionString)
        {
            try
            {
                return await ConnectionMultiplexer.ConnectAsync(connectionString);
            }
            catch (Exception)
            {
                throw new Exception("Failed to create connection to cache.");
            }

        }

        /// <summary>
        /// Closes redis cache multiplexer connection.
        /// </summary>
        internal async Task CloseMultiplexerAsync(IConnectionMultiplexer existingMultiplexer)
        {
            try
            {
                await existingMultiplexer.CloseAsync();
                await existingMultiplexer.DisposeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Failed to close connection to cache.");
            }
        }

        /// <summary>
        /// Process message from channel by building a RedisMessageModel and triggering the function.
        /// </summary>
        internal async Task ProcessMessageAsync(RedisTriggerType triggerType, string trigger, string message, CancellationToken cancellationtoken)
        {
            var callBack = new RedisMessageModel
            {
                TriggerType = triggerType,
                Trigger = trigger,
                Message = message
            };

            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = callBack }, cancellationtoken);
        }

        /// <summary>
        /// Subscribes to keyspace notification channel.
        /// </summary>
        internal async Task EnableKeySpaceAsync(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            ChannelMessageQueue channel = await multiplexer.GetSubscriber().SubscribeAsync(String.Format(KEYSPACE_TEMPLATE, trigger));
            channel.OnMessage(async (msg) =>
            {
                string rawChannel = msg.Channel;
                string key = rawChannel.Substring(rawChannel.IndexOf(':') + 1);
                await ProcessMessageAsync(triggerType, key, msg.Message, cancellationToken);
            });
        }

        /// <summary>
        /// Subscribes to keyevent notification channel.
        /// </summary>
        internal async Task EnableKeyEventAsync(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            ChannelMessageQueue channel = await multiplexer.GetSubscriber().SubscribeAsync(String.Format(KEYEVENT_TEMPLATE, trigger));
            channel.OnMessage(async (msg) =>
            {
                string rawChannel = msg.Channel;
                string keyevent = rawChannel.Substring(rawChannel.IndexOf(':') + 1);
                await ProcessMessageAsync(triggerType, keyevent, msg.Message, cancellationToken);
            });
        }

        /// <summary>
        /// Subscribes to pubsub channel.
        /// </summary>
        internal async Task EnablePubSubAsync(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            ChannelMessageQueue channel = await multiplexer.GetSubscriber().SubscribeAsync(trigger);
            channel.OnMessage(async (msg) =>
            {
                await ProcessMessageAsync(triggerType, msg.Channel, msg.Message, cancellationToken);
            });
        }
    }
}
