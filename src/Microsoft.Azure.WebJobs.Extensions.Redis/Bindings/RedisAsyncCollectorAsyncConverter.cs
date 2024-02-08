using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Azure;
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
        private AzureComponentFactory azureComponentFactory;
        private ILogger logger;

        public RedisAsyncCollectorAsyncConverter(IConfiguration configuration, AzureComponentFactory azureComponentFactory, ILogger logger)
        {
            this.configuration = configuration;
            this.azureComponentFactory = azureComponentFactory;
            this.logger = logger;
        }

        public async Task<IAsyncCollector<string>> ConvertAsync(RedisAttribute input, CancellationToken cancellationToken)
        {
            IConnectionMultiplexer multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, azureComponentFactory, input.ConnectionStringSetting, RedisUtilities.RedisOutputBinding);
            return new RedisAsyncCollector(multiplexer, input.Command, logger);
        }
    }
}
