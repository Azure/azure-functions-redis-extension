using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisAsyncCollector : IAsyncCollector<string[]>
    {
        private readonly IConnectionMultiplexer multiplexer;
        private readonly string command;
        private readonly ILogger logger;

        private ITransaction transaction;

        public RedisAsyncCollector(IConnectionMultiplexer multiplexer, string command, ILogger logger)
        {
            this.multiplexer = multiplexer;
            this.command = command;
            this.logger = logger;
        }

        public Task AddAsync(string[] arguments, CancellationToken cancellationToken = default)
        {
            if (transaction is null)
            {
                logger?.LogDebug("Creating transaction.");
                transaction = multiplexer.GetDatabase().CreateTransaction();
            }

            logger?.LogDebug($"Adding {command} command to transaction with input string[] arguments.");
            _ = transaction.ExecuteAsync(command, arguments, CommandFlags.FireAndForget);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            logger?.LogDebug("Executing transaction.");
            await transaction.ExecuteAsync(CommandFlags.FireAndForget);
            transaction = null;
            return;
        }
    }
}
