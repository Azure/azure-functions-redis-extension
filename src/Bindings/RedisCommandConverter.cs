using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisCommandConverter : IAsyncConverter<RedisCommandAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;

        public RedisCommandConverter(IConfiguration configuration, INameResolver nameResolver) {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
        }

        public async Task<RedisResult> ConvertAsync(RedisCommandAttribute input, CancellationToken cancellationToken) {
            string connectionString = RedisUtilities.ResolveConnectionString(configuration, input.ConnectionStringSetting);
            string commandString = RedisUtilities.ResolveString(nameResolver, input.Command, nameof(input.Command));
            string[] stringArgs = RedisUtilities.ResolveDelimitedString(nameResolver, input.Args, nameof(input.Args));
            object[] args = stringArgs.Select(arg => (object) arg).ToArray();

            return await (await ConnectionMultiplexer.ConnectAsync(connectionString)).GetDatabase().ExecuteAsync(commandString, args);
        }
    }
}
