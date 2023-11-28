using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisTriggerBindingProviderTests
    {
        [Fact]
        public async Task Constructor_NullContext_ThrowsExceptionAsync()
        {
            RedisPubSubTriggerBindingProvider bindingProvider = new RedisPubSubTriggerBindingProvider(A.Fake<IConfiguration>(), A.Fake<INameResolver>(), A.Fake<ILogger>()); ;
            await Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
        }
    }
}
