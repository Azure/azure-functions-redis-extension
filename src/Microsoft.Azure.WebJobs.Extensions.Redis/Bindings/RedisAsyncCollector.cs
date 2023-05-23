using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisAsyncCollector : IAsyncCollector<string[]>
    {
        private readonly IConnectionMultiplexer multiplexer;
        private readonly string attributeCommand;
        private readonly ILogger logger;

        private ITransaction transaction;

        public RedisAsyncCollector(IConnectionMultiplexer multiplexer, string attributeCommand, ILogger logger)
        {
            this.multiplexer = multiplexer;
            this.attributeCommand = attributeCommand;
            this.logger = logger;
        }

        public Task AddAsync(string[] arguments, CancellationToken cancellationToken = default)
        {
            if (transaction is null)
            {
                transaction = multiplexer.GetDatabase().CreateTransaction();
            }

            if (string.IsNullOrWhiteSpace(attributeCommand) && arguments.Length > 1)
            {
                logger?.LogDebug($"Adding command to transaction using only input string[] arguments.");
                _ = transaction.ExecuteAsync(arguments[0], new ArraySegment<string>(arguments, 1, arguments.Length - 1));
            }
            else
            {
                logger?.LogDebug($"Adding command to transaction using given command with input string[] arguments.");
                _ = transaction.ExecuteAsync(attributeCommand, arguments);
            }
            return Task.CompletedTask;
        }

        public Task AddAsync(byte[][] arguments, CancellationToken cancellationToken = default)
        {
            if (transaction is null)
            {
                transaction = multiplexer.GetDatabase().CreateTransaction();
            }

            logger?.LogDebug($"Adding command to transaction using given command with input byte[][] arguments.");
            _ = transaction.ExecuteAsync(attributeCommand, arguments);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            logger?.LogDebug("Executing transaction.");
            bool result = transaction.Execute();
            transaction = null;
            return Task.FromResult(result);
        }
    }
}
