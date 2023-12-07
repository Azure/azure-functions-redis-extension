using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

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

        private static IConfiguration localsettings = new ConfigurationBuilder().AddJsonFile(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "local.settings.json")).Build();

        [Fact]
        public void ResolveConnectionString_ValidConnectionStringSetting_ReturnsResolvedString()
        {
            Assert.Equal("127.0.0.1:6379", RedisUtilities.ResolveConnectionString(localsettings, "redisConnectionString"));
        }

        [Fact]
        public void ResolveConnectionString_ValidSetting_ReturnsResolvedString()
        {
            Assert.Equal("testCacheString", RedisUtilities.ResolveConnectionString(testConfig, "CacheConnection"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ResolveConnectionString_EmptyConnectionStringSetting_ThrowsArgumentNullException(string connectionStringSetting)
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveConnectionString(testConfig, connectionStringSetting));
        }

        [Fact]
        public void ResolveConnectionString_InvalidSetting_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RedisUtilities.ResolveConnectionString(testConfig, "invalidSetting"));
        }

        [Fact]
        public void ResolveString_ValidSetting_ReturnsResolvedString()
        {
            Assert.Equal("PubSub", RedisUtilities.ResolveString(new DefaultNameResolver(testConfig), "%pubsubkey%", "testSetting")); ;
        }

        [Theory]
        [InlineData("%invalidStringKey%")]
        [InlineData("hello")]
        public void ResolveString_InvalidSetting_ReturnsInput(string setting)
        {
            Assert.Equal(setting, RedisUtilities.ResolveString(new DefaultNameResolver(testConfig), setting, "testSetting")); ;
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ResolveString_EmptySetting_ThrowsArgumentNullException(string setting)
        {
            Assert.Throws<ArgumentNullException>(() => RedisUtilities.ResolveString(new DefaultNameResolver(testConfig), setting, "testSetting"));
        }
    }
}
