using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Bindings
{
    internal class RedisConverter : IConverter<RedisAttribute, IConnectionMultiplexer>
    {
        private readonly IConfiguration configuration;

        public RedisConverter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IConnectionMultiplexer Convert(RedisAttribute input)
        {
            return ConnectionMultiplexer.Connect(input.ConnectionString);
        }
    }
}
