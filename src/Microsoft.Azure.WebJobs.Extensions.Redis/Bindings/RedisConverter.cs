using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisConverter<T> : IAsyncConverter<RedisAttribute, T>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisConverter(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.logger = logger;
        }

        public async Task<T> ConvertAsync(RedisAttribute input, CancellationToken cancellationToken)
        {
            string command = RedisUtilities.ResolveString(nameResolver, input.Command, nameof(input.Command));
            IDatabase db = RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexer(configuration, input.ConnectionStringSetting).GetDatabase();

            string[] arguments = command.Split(' ');
            switch (arguments[0])
            {
                case "GET": return await GET(db, arguments);
                case "HGET": return await HGET(db, arguments);
                default:
                    throw new ArgumentException($"Command '{arguments[0]}' not supported for Redis Input Binding.");
            }
        }

        private async Task<T> GET(IDatabase db, string[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new ArgumentException($"Command '{arguments[0]}' requires 1 argument.");
            }
            RedisValue value = await db.StringGetAsync(arguments[1]);
            return (T)RedisUtilities.RedisValueTypeConverter(value, typeof(T));
        }

        private async Task<T> HGET(IDatabase db, string[] arguments)
        {
            if (arguments.Length != 3)
            {
                throw new ArgumentException($"Command '{arguments[0]}' requires 2 arguments.");
            }
            RedisValue value = await db.HashGetAsync(arguments[1], arguments[2]);
            return (T)RedisUtilities.RedisValueTypeConverter(value, typeof(T));
        }
    }
}