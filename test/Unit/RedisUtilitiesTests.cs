using Xunit;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisUtilitiesTests
    {
        private static IConfiguration testConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheConnection", "testCacheString" },
            { "ChannelName", "testChannelName" },
            { "pubsubkey", "PubSub" },
            { "keyspacekey", "KeySpace" },
            { "keyeventkey", "KeyEvent" },
            { "randomkey", "random" },
            { "int100key", "100" },
            { "int-5key", "-5" },
            { "int2key", "2" },
            { "trueKey", "true" },
            { "falseKey", "false" }
        }).Build();

        [Theory]
        [InlineData("%CacheConnection%", "testCacheString")]
        [InlineData("testCacheString", "testCacheString")]
        public void ResolveString_ValidString_ReturnsResolvedString(string cacheString, string expectedResult)
        {
            Assert.Equal(expectedResult, RedisUtilities.ResolveString(testConfig, cacheString, ""));
        }

        [Fact]
        public void ResolveString_InvalidKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RedisUtilities.ResolveString(testConfig, "%invalidStringKey%", ""));
        }

        [Fact]
        public void ResolveString_EmptyString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveString(testConfig, "", ""));
        }

        [Theory]
        [InlineData("%pubsubkey%", RedisTriggerType.PubSub)]
        [InlineData("%keyspacekey%", RedisTriggerType.KeySpace)]
        [InlineData("%keyeventkey%", RedisTriggerType.KeyEvent)]
        public void ResolveTriggerType_ValidTriggerType_ReturnsResolvedTriggerType(string cacheString, RedisTriggerType expectedResult)
        {
            Assert.Equal(expectedResult, RedisUtilities.ResolveTriggerType(testConfig, cacheString, ""));
        }

        [Fact]
        public void ResolveTriggerType_InvalidKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RedisUtilities.ResolveTriggerType(testConfig, "%invalidStringKey%", ""));
        }

        [Fact]
        public void ResolveTriggerType_InvalidTriggerType_ThrowsInvalidCastException()
        {
            Assert.Throws<InvalidCastException>(() => RedisUtilities.ResolveTriggerType(testConfig, "%randomKey%", ""));
        }

        [Fact]
        public void ResolveTriggerType_EmptyTriggerType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveTriggerType(testConfig, "", ""));
        }

        [Theory]
        [InlineData("%int100key%", 100)]
        [InlineData("%int2key%", 2)]
        public void ResolveInt_ValidInt_ReturnsResolvedInt(string cacheString, int expectedResult)
        {
            Assert.Equal(expectedResult, RedisUtilities.ResolveInt(testConfig, cacheString, ""));
        }

        [Fact]
        public void ResolveInt_InvalidKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RedisUtilities.ResolveInt(testConfig, "%invalidStringKey%", ""));
        }

        [Fact]
        public void ResolveInt_NegativeInt_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RedisUtilities.ResolveInt(testConfig, "%int-5key%", ""));
        }

        [Fact]
        public void ResolveInt_InvalidInt_ThrowsInvalidCastException()
        {
            Assert.Throws<InvalidCastException>(() => RedisUtilities.ResolveInt(testConfig, "%randomKey%", ""));
        }

        [Fact]
        public void ResolveInt_EmptyInt_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveInt(testConfig, "", ""));
        }

        [Theory]
        [InlineData("%trueKey%", true)]
        [InlineData("%falseKey%", false)]
        public void ResolveBool_ValidBool_ReturnsResolvedBool(string cacheString, bool expectedResult)
        {
            Assert.Equal(expectedResult, RedisUtilities.ResolveBool(testConfig, cacheString, ""));
        }

        [Fact]
        public void ResolveBool_InvalidKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => RedisUtilities.ResolveBool(testConfig, "%invalidStringKey%", ""));
        }

        [Fact]
        public void ResolveBool_InvalidBool_ThrowsInvalidCastException()
        {
            Assert.Throws<InvalidCastException>(() => RedisUtilities.ResolveBool(testConfig, "%randomKey%", ""));
        }

        [Fact]
        public void ResolveBool_EmptyBool_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveBool(testConfig, "", ""));
        }
    }
}
