using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisAsyncConverter<T> : IAsyncConverter<RedisAttribute, T>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisAsyncConverter(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.logger = logger;
        }

        public async Task<T> ConvertAsync(RedisAttribute input, CancellationToken cancellationToken)
        {
            string fullCommand = RedisUtilities.ResolveString(nameResolver, input.Command, nameof(input.Command));
            IDatabase db = RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexer(configuration, input.ConnectionStringSetting).GetDatabase();

            string[] splitCommand = fullCommand.Split(RedisUtilities.BindingDelimiter);
            string command = splitCommand[0];
            if (!SingleOutputReadCommands.Contains(command))
            {
                throw new ArgumentException($"Command '{command}' not supported for Redis Input Binding.");
            }

            string[] arguments = splitCommand.Skip(1).ToArray();

            logger?.LogDebug($"Executing '{fullCommand}'.");
            RedisResult result = await db.ExecuteAsync(command, arguments);
            return (T)RedisResultTypeConverter(result, typeof(T));
        }

        internal static object RedisResultTypeConverter(RedisResult value, Type destinationType)
        {
            switch (value.Type)
            {
                case ResultType.None:
                    return null;
                case ResultType.Error:
                    throw new RedisException((string)value);
                case ResultType.SimpleString:
                case ResultType.Integer:
                case ResultType.BulkString:
                    return RedisUtilities.RedisValueTypeConverter((RedisValue)value, destinationType);
                default:
                    throw new InvalidOperationException($"Redis Output BindingResultTypeConverter does not support RedisResult type '{value.Type}'.");
            }
        }

        internal static readonly HashSet<string> SingleOutputReadCommands = new HashSet<string>()
        {
            "BITCOUNT",
            "BITPOS",
            "DBSIZE",
            "DUMP",
            "EXISTS",
            "EXPIRETIME",
            "GEODIST",
            "GET",
            "GETBIT",
            "GETRANGE",
            "HEXISTS",
            "HGET",
            "HLEN",
            "HSTRLEN",
            "LINDEX",
            "LLEN",
            "PEXPIRETIME",
            "PFCOUNT",
            "PTTL",
            "RANDOMKEY",
            "SCARD",
            "SINTERCARD",
            "SISMEMBER",
            "STRLEN",
            "SUBSTR",
            "TOUCH",
            "TTL",
            "TYPE",
            "XLEN",
            "ZCARD",
            "ZCOUNT",
            "ZDIFF",
            "ZINTERCARD",
            "ZLEXCOUNT",
            "ZSCORE",
        };
    }
}