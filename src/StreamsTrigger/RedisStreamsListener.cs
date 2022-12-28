using System;
using System.Linq;
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
    internal sealed class RedisStreamsListener : IListener
    {
        internal const int DEFAULT_POLLING_INTERVAL_MS = 1000;

        internal string connectionString;
        internal StreamPosition[] positions;
        internal string trigger;
        internal string consumerGroup;
        internal string consumerName;
        internal int count;
        internal TimeSpan pollingInterval;
        internal ITriggeredFunctionExecutor executor;

        internal IConnectionMultiplexer multiplexer;

        public RedisStreamsListener(string connectionString, string[] keys, string consumerGroup, string consumerName, int count, int pollingInterval, ITriggeredFunctionExecutor executor)
        {
            this.connectionString = connectionString;
            this.positions = keys.Select(key => new StreamPosition(key, 0)).ToArray();
            this.consumerGroup = consumerGroup;
            this.consumerName = consumerName;
            this.count = count;
            this.pollingInterval = TimeSpan.FromMilliseconds(pollingInterval);
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            multiplexer = InitializeConnectionMultiplexer(connectionString);
            if (multiplexer.IsConnected)
            {
                IDatabase db = multiplexer.GetDatabase();
                if (String.IsNullOrEmpty(consumerGroup) && String.IsNullOrEmpty(consumerName))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        RedisStream[] streams = await db.StreamReadAsync(positions, count);
                        if (streams.Length == 0)
                        {
                            await Task.Delay(pollingInterval);
                        }
                        else
                        {
                            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = streams }, cancellationToken);
                            foreach (RedisStream stream in streams)
                            {
                                await db.StreamDeleteAsync(stream.Key, stream.Entries.Select(entry => entry.Id).ToArray());
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(consumerGroup) && !String.IsNullOrEmpty(consumerName))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        RedisStream[] streams = await multiplexer.GetDatabase().StreamReadGroupAsync(positions, consumerGroup, consumerName, count);
                        if (streams.Length == 0)
                        {
                            await Task.Delay(pollingInterval);
                        }
                        else
                        {
                            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = streams }, cancellationToken);
                            foreach (RedisStream stream in streams)
                            {
                                await db.StreamAcknowledgeAsync(stream.Key, consumerGroup, stream.Entries.Select(entry => entry.Id).ToArray());
                            }

                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("Failed to connect to cache.");
            }
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
    }
}
