using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisAsyncCollector : IAsyncCollector<string>
    {
        private readonly string command;
        private readonly ILogger logger;
        private readonly IBatch batch;

        public RedisAsyncCollector(IConnectionMultiplexer multiplexer, string command, ILogger logger)
        {
            this.command = command;
            this.logger = logger;
            this.batch = multiplexer.GetDatabase().CreateBatch();
        }

        public Task AddAsync(string argument, CancellationToken cancellationToken = default)
        {
            logger?.LogDebug($"Batching '{command} {argument}'.");
            _ = batch.ExecuteAsync(command, argument.Split(RedisUtilities.BindingDelimiter), CommandFlags.FireAndForget);
            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            logger?.LogDebug("Executing batch.");
            batch.Execute();
            return Task.CompletedTask;
        }
    }
}
 