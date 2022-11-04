using Xunit;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisTriggerBindingProviderTests
    {
        private static IConfiguration testConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheConnection", "testCacheConnectionString" },
            { "ChannelName", "testChannelName" },
        }).Build();

        [Fact]
        public void Constructor_NullContext_ThrowsException()
        {
            RedisTriggerBindingProvider bindingProvider = new RedisTriggerBindingProvider(testConfig);
            //var expectedException = Record.ExceptionAsync(() => bindingProvider.TryCreateAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
            //Assert.IsType<ArgumentNullException>(expectedException);
        }


        [Theory]
        [InlineData("%CacheConnection%", "testCacheConnectionString")]
        [InlineData("testCacheConnectionString", "testCacheConnectionString")]
        public void ResolveConnectionString_ValidConnectionString_ReturnsResolvedConnectionString(string cacheConnectionString, string expectedResult)
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                ConnectionString = cacheConnectionString
            };

            string resolvedConnectionString = triggerProvider.ResolveConnectionString(unresolvedRedisTriggerAttribute);

            Assert.Equal(expectedResult, resolvedConnectionString);
        }

        [Fact]
        public void ResolveConnectionString_InvalidConnectionStringKey_ThrowsArgumentException()
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                ConnectionString = "%invalidConnectionStringKey%"
            };

            var expectedException = Record.Exception(() => triggerProvider.ResolveConnectionString(unresolvedRedisTriggerAttribute));

            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentException>(expectedException);
        }

        [Fact]
        public void ResolveConnectionString_EmptyConnectionString_ThrowsArgumentNullException()
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                ConnectionString = ""
            };

            var expectedException = Record.Exception(() => triggerProvider.ResolveConnectionString(unresolvedRedisTriggerAttribute));

            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentNullException>(expectedException);
        }

        [Theory]
        [InlineData("%ChannelName%", "testChannelName")]
        [InlineData("testChannelName", "testChannelName")]
        public void ResolveTrigger_ValidTrigger_ReturnsResolvedTrigger(string channelName, string expectedResult)
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                Trigger = channelName
            };

            string resolvedChannelName = triggerProvider.ResolveTrigger(unresolvedRedisTriggerAttribute);

            Assert.Equal(expectedResult, resolvedChannelName);
        }

        [Fact]
        public void ResolveTrigger_InvalidTrigger_ThrowsArgumentException()
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                Trigger = "%invalidChannelNameKey%"
            };

            var expectedException = Record.Exception(() => triggerProvider.ResolveTrigger(unresolvedRedisTriggerAttribute));

            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentException>(expectedException);
        }

        [Fact]
        public void ResolveTrigger_EmptyTrigger_ThrowsArgumentNullException()
        {
            RedisTriggerBindingProvider triggerProvider = new RedisTriggerBindingProvider(testConfig);
            RedisTriggerAttribute unresolvedRedisTriggerAttribute = new RedisTriggerAttribute
            {
                Trigger = ""
            };

            var expectedException = Record.Exception(() => triggerProvider.ResolveTrigger(unresolvedRedisTriggerAttribute));

            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentNullException>(expectedException);
        }

    }
}
