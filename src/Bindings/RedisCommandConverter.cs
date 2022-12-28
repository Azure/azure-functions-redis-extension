using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisCommandConverter : IConverter<RedisCommandAttribute, RedisResult>
    {
        private readonly IConfiguration configuration;

        public RedisCommandConverter(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public RedisResult Convert(RedisCommandAttribute input)
        {
            return ConnectionMultiplexer.Connect(input.ConnectionString).GetDatabase().Execute(input.RedisCommand, args: input.Arguments.Split(' '));
        }
    }
}
