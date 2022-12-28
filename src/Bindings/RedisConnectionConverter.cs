using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisConnectionConverter : IConverter<RedisConnectionAttribute, IConnectionMultiplexer>
    {
        private readonly IConfiguration configuration;

        public RedisConnectionConverter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IConnectionMultiplexer Convert(RedisConnectionAttribute input)
        {
            return ConnectionMultiplexer.Connect(input.ConnectionString);
        }
    }
}
