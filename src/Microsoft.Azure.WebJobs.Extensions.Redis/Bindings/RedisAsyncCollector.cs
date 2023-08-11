﻿using System.Threading;
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
        private readonly IBatch batch;

        public RedisAsyncCollector(IConnectionMultiplexer multiplexer, string command, ILogger logger)
        {
            this.multiplexer = multiplexer;
            this.command = command;
            this.logger = logger;
            this.batch = multiplexer.GetDatabase().CreateBatch();
        }

        public Task AddAsync(string[] arguments, CancellationToken cancellationToken = default)
        {
            logger?.LogDebug($"Adding {command} command to batch with input string[] arguments.");
            _ = batch.ExecuteAsync(command, arguments, CommandFlags.FireAndForget);
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