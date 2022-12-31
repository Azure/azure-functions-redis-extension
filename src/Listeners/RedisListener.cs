using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisListener : IListener
    {
        internal const string KEYSPACE_TEMPLATE = "__keyspace@*__:{0}";
        internal const string KEYEVENT_TEMPLATE = "__keyevent@*__:{0}";

        internal ITriggeredFunctionExecutor executor;
        internal string connectionString;
        internal string trigger;
        internal RedisTriggerType triggerType;
        internal IConnectionMultiplexer multiplexer;

        public RedisListener(string connectionString, RedisTriggerType triggerType, string trigger, ITriggeredFunctionExecutor executor)
        {
            this.connectionString = connectionString;
            this.triggerType = triggerType;
            this.trigger = trigger;
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            multiplexer = InitializeConnectionMultiplexer(connectionString);
            if (multiplexer.IsConnected)
            {
                switch (triggerType)
                {
                    case RedisTriggerType.KeySpace: 
                        EnableKeySpace(multiplexer, cancellationToken);
                        break;
                    case RedisTriggerType.KeyEvent: 
                        EnableKeyEvent(multiplexer, cancellationToken);
                        break;
                    case RedisTriggerType.PubSub:
                        EnablePubSub(multiplexer, cancellationToken);
                        break;
                    default: 
                        break;
                }
            }
            else
            {
                throw new ArgumentException("Failed to connect to cache.");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggers disconnect from cache when cancellation token is invoked.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            CloseMultiplexer(multiplexer);
            return Task.CompletedTask;
        }

        public void Cancel()
        {
            CloseMultiplexer(multiplexer);
        }

        public void Dispose()
        {
            CloseMultiplexer(multiplexer);
        }

        /// <summary>
        /// Creates redis cache multiplexer connection.
        /// </summary>
        private static IConnectionMultiplexer InitializeConnectionMultiplexer(string connectionString)
        {
            try
            {
                return ConnectionMultiplexer.Connect(connectionString);
            }
            catch (Exception)
            {
                throw new Exception("Failed to create connection to cache.");
            }

        }

        /// <summary>
        /// Closes redis cache multiplexer connection.
        /// </summary>
        internal void CloseMultiplexer(IConnectionMultiplexer existingMultiplexer)
        {
            try
            {
                existingMultiplexer.Close();
                existingMultiplexer.Dispose();
            }
            catch (Exception)
            {
                throw new Exception("Failed to close connection to cache.");
            }
        }

        /// <summary>
        /// Process message from channel by building a RedisMessageModel & triggering the function.
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
        internal void EnableKeySpace(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            multiplexer.GetSubscriber().Subscribe(String.Format(KEYSPACE_TEMPLATE, trigger)).OnMessage(async (msg) =>
            {
                string rawChannel = msg.Channel;
                string keyspace = rawChannel.Substring(rawChannel.IndexOf(':') + 1);
                await ProcessMessageAsync(triggerType, keyspace, msg.Message, cancellationToken);
            });
        }

        /// <summary>
        /// Subscribes to keyevent notification channel.
        /// </summary>
        internal void EnableKeyEvent(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            multiplexer.GetSubscriber().Subscribe(String.Format(KEYEVENT_TEMPLATE, trigger)).OnMessage(async (msg) =>
            {
                string rawChannel = msg.Channel;
                string keyevent = rawChannel.Substring(rawChannel.IndexOf(':') + 1);
                await ProcessMessageAsync(triggerType, keyevent, msg.Message, cancellationToken);
            });
        }

        /// <summary>
        /// Subscribes to pubsub channel.
        /// </summary>
        internal void EnablePubSub(IConnectionMultiplexer multiplexer, CancellationToken cancellationToken)
        {
            multiplexer.GetSubscriber().Subscribe(trigger).OnMessage(async (msg) =>
            {
                await ProcessMessageAsync(triggerType, msg.Channel, msg.Message, cancellationToken);
            });
        }
    }
}
