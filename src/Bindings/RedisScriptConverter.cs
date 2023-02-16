using System.Linq;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisScriptConverter : IConverter<RedisScriptAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;

        public RedisScriptConverter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public RedisResult Convert(RedisScriptAttribute input)
        {
            RedisKey[] keys = string.IsNullOrEmpty(input.Keys) ? null : input.Keys.Split('_').Select(key => new RedisKey(key)).ToArray();
            RedisValue[] values = string.IsNullOrEmpty(input.Values) ? null : input.Values.Split('_').Select(key => new RedisValue(key)).ToArray();
            return ConnectionMultiplexer.Connect(input.ConnectionString).GetDatabase().ScriptEvaluate(input.LuaScript, keys, values);
        }
    }
}
