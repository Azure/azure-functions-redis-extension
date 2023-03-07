using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisScriptConverter : IAsyncConverter<RedisScriptAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;

        public RedisScriptConverter(IConfiguration configuration, INameResolver nameResolver)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
        }

        public async Task<RedisResult> ConvertAsync(RedisScriptAttribute input, CancellationToken cancellationToken) {
            string connectionString = RedisUtilities.ResolveConnectionString(configuration, input.ConnectionStringSetting);
            string scriptString = RedisUtilities.ResolveString(nameResolver, input.LuaScript, nameof(input.LuaScript));
            string[] stringKeys = RedisUtilities.ResolveDelimitedString(nameResolver, input.Keys, nameof(input.Keys));
            string[] stringArgs = RedisUtilities.ResolveDelimitedString(nameResolver, input.Args, nameof(input.Args));

            RedisKey[] keys = stringKeys.Select(key => new RedisKey(key)).ToArray();
            RedisValue[] args = stringArgs.Select(arg => new RedisValue(arg)).ToArray();

            return await (await ConnectionMultiplexer.ConnectAsync(connectionString)).GetDatabase().ScriptEvaluateAsync(scriptString, keys, args);
        }
    }
}
