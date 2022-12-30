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
    /// Responsible for polling a cache.
    /// </summary>
    internal abstract class RedisPollingListenerBase : IListener
    {
        internal string connectionString;
        internal TimeSpan pollingInterval;
        internal RedisKey[] keys;
        internal int count;
        internal ITriggeredFunctionExecutor executor;
        internal IConnectionMultiplexer multiplexer;

        private bool isConnected = false;

        public RedisPollingListenerBase(string connectionString, int pollingInterval, string keys, int count, ITriggeredFunctionExecutor executor)
        {
            this.connectionString = connectionString;
            this.pollingInterval = TimeSpan.FromMilliseconds(pollingInterval);
            this.keys = keys.Split('_').Select(key => new RedisKey(key)).ToArray();
            this.count = count;
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (isConnected)
            {
                return;
            }

            multiplexer = await InitializeConnectionMultiplexerAsync(connectionString);
            if (multiplexer.IsConnected)
            {
                isConnected = true;
                _ = Task.Run(() => Loop(cancellationToken));
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
        /// Implementation of the logic used to poll the cache.
        /// </summary>
        /// <returns>If there should be a delay before the next poll.</returns>
        public abstract Task<bool> PollAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Main loop thread.
        /// </summary>
        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await PollAsync(cancellationToken))
                {
                    await Task.Delay(pollingInterval);
                }
            }
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
        private static async void CloseMultiplexer(IConnectionMultiplexer existingMultiplexer)
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
    }
}
