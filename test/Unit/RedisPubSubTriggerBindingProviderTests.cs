using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using FakeItEasy;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisTriggerBindingProviderTests
    {
        [Fact]
        public void Constructor_NullContext_ThrowsException()
        {
            RedisPubSubTriggerBindingProvider bindingProvider = new RedisPubSubTriggerBindingProvider(A.Fake<IConfiguration>(), A.Fake<INameResolver>(), A.Fake<ILogger>());
            Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
        }
    }
}
