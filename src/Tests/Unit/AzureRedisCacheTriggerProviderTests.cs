using Xunit;
using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache.Tests.Unit
{
    public class AzureRedisCacheTriggerProviderTests
    {
        /// <summary>
        /// Test naming format : MethodName_StateUnderTest_ExpectedBehaviour
        /// </summary>

        readonly static IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheConnection", "testCacheConnectionString" },
            { "ChannelName", "testChannelName" }
        }).Build();

        readonly AzureRedisCacheTriggerProvider triggerProvider = new AzureRedisCacheTriggerProvider(config);

        [Theory]
        [InlineData("%CacheConnection%", "testCacheConnectionString")]
        [InlineData("testCacheConnectionString","testCacheConnectionString")]
        public void ResolveConnectionString_ValidConnectionString_ReturnsResolvedConnectionString(string cacheConnectionString, string expectedResult)
        {
            // arrange
            AzureRedisCacheTriggerAttribute unresolvedAzureRedisCacheTriggerAttribute = new AzureRedisCacheTriggerAttribute
            {
                CacheConnection = cacheConnectionString
            };

            // act
            string resolvedConnectionString = triggerProvider.ResolveConnectionString(unresolvedAzureRedisCacheTriggerAttribute);
           
            // assert
            Assert.Equal(expectedResult , resolvedConnectionString);
        }

        [Fact]
        public void ResolveConnectionString_EmptyConnectionString_ThrowsArgumentNullException()
        {
            // arrange
            AzureRedisCacheTriggerAttribute unresolvedAzureRedisCacheTriggerAttribute = new AzureRedisCacheTriggerAttribute
            {
                CacheConnection = ""
            };

            // act
            var expectedException = Record.Exception(() => triggerProvider.ResolveConnectionString(unresolvedAzureRedisCacheTriggerAttribute));

            // assert
            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentNullException>(expectedException);
        }

        [Theory]
        [InlineData("%ChannelName%", "testChannelName")]
        [InlineData("testChannelName", "testChannelName")]
        public void ResolveChannelName_ValidChannelName_ReturnsResolvedChannelName(string channelName, string expectedResult)
        {
            // arrange
            AzureRedisCacheTriggerAttribute unresolvedAzureRedisCacheTriggerAttribute = new AzureRedisCacheTriggerAttribute
            {
                ChannelName = channelName
            };

            // act
            string resolvedChannelName = triggerProvider.ResolveChannelName(unresolvedAzureRedisCacheTriggerAttribute);

            // assert
            Assert.Equal(expectedResult, resolvedChannelName);
        }

        [Fact]
        public void ResolveChannelName_EmptyChannelName_ThrowsArgumentNullException()
        {
            // arrange
            AzureRedisCacheTriggerAttribute unresolvedAzureRedisCacheTriggerAttribute = new AzureRedisCacheTriggerAttribute
            {
                ChannelName = ""
            };

            // act
            var expectedException = Record.Exception(() => triggerProvider.ResolveChannelName(unresolvedAzureRedisCacheTriggerAttribute));

            // assert
            Assert.NotNull(expectedException);
            Assert.IsType<ArgumentNullException>(expectedException);
        }
}
}
