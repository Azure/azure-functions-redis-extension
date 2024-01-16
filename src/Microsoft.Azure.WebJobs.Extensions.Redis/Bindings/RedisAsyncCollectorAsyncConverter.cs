using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Used to create the RedisAsyncCollector.
    /// </summary>
    internal class RedisAsyncCollectorAsyncConverter : IAsyncConverter<RedisAttribute, IAsyncCollector<string>>
    {
        private IConfiguration configuration;
        private ILogger logger;

        public RedisAsyncCollectorAsyncConverter(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<IAsyncCollector<string>> ConvertAsync(RedisAttribute input, CancellationToken cancellationToken)
        {
            IConnectionMultiplexer multiplexer = RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexer(configuration, input.ConnectionStringSetting);
            return new RedisAsyncCollector(multiplexer, input.Command, logger);
        }
    }
}
