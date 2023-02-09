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
            RedisPubSubTriggerBindingProvider bindingProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            //var expectedException = Record.ExceptionAsync(() => bindingProvider.TryCreateAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => bindingProvider.TryCreateAsync(null));
            //Assert.IsType<ArgumentNullException>(expectedException);
        }


        [Theory]
        [InlineData("%CacheConnection%", "testCacheConnectionString")]
        [InlineData("testCacheConnectionString", "testCacheConnectionString")]
        public void ResolveConnectionString_ValidConnectionString_ReturnsResolvedConnectionString(string cacheConnectionString, string expectedResult)
        {
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
            {
                ConnectionString = cacheConnectionString
            };

            string resolvedConnectionString = triggerProvider.ResolveConnectionString(unresolvedRedisTriggerAttribute);

            Assert.Equal(expectedResult, resolvedConnectionString);
        }

        [Fact]
        public void ResolveConnectionString_InvalidConnectionStringKey_ThrowsArgumentException()
        {
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
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
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
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
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
            {
                Trigger = channelName
            };

            string resolvedChannelName = triggerProvider.ResolveTrigger(unresolvedRedisTriggerAttribute);

            Assert.Equal(expectedResult, resolvedChannelName);
        }

        [Fact]
        public void ResolveTrigger_InvalidTrigger_ThrowsArgumentException()
        {
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
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
            RedisPubSubTriggerBindingProvider triggerProvider = new RedisPubSubTriggerBindingProvider(testConfig);
            RedisPubSubTriggerAttribute unresolvedRedisTriggerAttribute = new RedisPubSubTriggerAttribute
            {
                Trigger = ""
            };

            var expectedException = Record.Exception(() => triggerProvider.ResolveTrigger(unresolvedRedisTriggerAttribute));

            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentNullException>(expectedException);
        }

    }
}
