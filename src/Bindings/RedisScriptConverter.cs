using System.Linq;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisScriptConverter : IConverter<RedisScriptAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;

        public RedisScriptConverter(IConfiguration configuration, INameResolver nameResolver)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
        }

        public RedisResult Convert(RedisScriptAttribute input)
        {
            string connectionString = RedisUtilities.ResolveConnectionString(configuration, input.ConnectionStringSetting);
            string scriptString = RedisUtilities.ResolveString(nameResolver, input.LuaScript, nameof(input.LuaScript));
            string[] stringKeys = RedisUtilities.ResolveDelimitedStrings(nameResolver, input.Keys, nameof(input.Keys));
            string[] stringArgs = RedisUtilities.ResolveDelimitedStrings(nameResolver, input.Args, nameof(input.Args));

            RedisKey[] keys = stringKeys.Select(key => new RedisKey(key)).ToArray();
            RedisValue[] args = stringKeys.Select(arg => new RedisValue(arg)).ToArray();

            return ConnectionMultiplexer.Connect(connectionString).GetDatabase().ScriptEvaluate(scriptString, keys, args);
        }
    }
}
