﻿using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisCommandConverter : IConverter<RedisCommandAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;

        public RedisCommandConverter(IConfiguration configuration, INameResolver nameResolver) {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
        }

        public RedisResult Convert(RedisCommandAttribute input)
        {
            string connectionString = RedisUtilities.ResolveConnectionString(configuration, input.ConnectionStringSetting);
            string commandString = RedisUtilities.ResolveString(nameResolver, input.Command, nameof(input.Command));
            string[] stringArgs = RedisUtilities.ResolveDelimitedStrings(nameResolver, input.Args, nameof(input.Args));
            RedisValue[] args = stringArgs.Select(arg => new RedisValue(arg)).ToArray();

            return ConnectionMultiplexer.Connect(connectionString).GetDatabase().Execute(commandString, args: args);
        }
    }
}
