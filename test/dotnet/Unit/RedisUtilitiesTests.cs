using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisUtilitiesTests
    {
        private static IConfiguration testConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheConnection", "0.0.0.0:6379" },
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
        public async void ResolveConfigurationOptionsAsync_ValidConnectionStringSetting_ReturnsResolvedString()
        {
            ConfigurationOptions options = await RedisUtilities.ResolveConfigurationOptionsAsync(localsettings, "redisConnectionString", "test");
            Assert.Single(options.EndPoints);
            Assert.Equal("127.0.0.1:6379", options.EndPoints[0].ToString());
        }

        [Fact]
        public async void ResolveConfigurationOptionsAsync_ValidSetting_ReturnsResolvedString()
        {

            ConfigurationOptions options = await RedisUtilities.ResolveConfigurationOptionsAsync(testConfig, "CacheConnection", "test");
            Assert.Single(options.EndPoints);
            Assert.Equal("0.0.0.0:6379", options.EndPoints[0].ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ResolveConfigurationOptionsAsync_EmptyConnectionStringSetting_ThrowsArgumentNullException(string connectionStringSetting)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RedisUtilities.ResolveConfigurationOptionsAsync(testConfig, connectionStringSetting, "test"));
        }

        [Fact]
        public async Task ResolveConfigurationOptionsAsync_InvalidSetting_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await RedisUtilities.ResolveConfigurationOptionsAsync(testConfig, "invalidSetting", "test"));
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
