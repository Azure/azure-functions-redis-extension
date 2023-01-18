using Xunit;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using FakeItEasy;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPubSubTriggerBindingProviderTests
    {
        [Fact]
        public void Constructor_NullContext_ThrowsException()
        {
            RedisPubSubTriggerBindingProvider bindingProvider = new RedisPubSubTriggerBindingProvider(A.Fake<IConfiguration>());
            Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
        }
    }
}
