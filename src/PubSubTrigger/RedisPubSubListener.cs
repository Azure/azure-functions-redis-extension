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
        internal ITriggeredFunctionExecutor executor;
        internal IRedisService redisService;
        internal string channel;

        public RedisPubSubListener(IRedisService redisService, string channel, ITriggeredFunctionExecutor executor)
        {
            this.redisService = redisService;
            this.channel = channel;
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            redisService.Connect();
            redisService.Subscribe(channel, async (msg) =>
            {
                var callBack = new RedisMessageModel
                {
                    Trigger = msg.Channel,
                    Message = msg.Message
                };

                await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = callBack }, cancellationToken);
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggers disconnect from cache when cancellation token is invoked.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            redisService.Close();
            return Task.CompletedTask;
        }

        public void Cancel()
        {
            redisService.Close();
        }

        public void Dispose()
        {
            redisService.Close();
        }
    }
}
