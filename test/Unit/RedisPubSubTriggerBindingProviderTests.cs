using System;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisTriggerBindingProviderTests
    {
        [Fact]
        public void Constructor_NullContext_ThrowsException()
        {
            RedisPubSubTriggerBindingProvider bindingProvider = new RedisPubSubTriggerBindingProvider(A.Fake<IConfiguration>(), A.Fake<ILogger>(), A.Fake<RedisExtensionConfigProvider>());
            Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
        }
    }
}
