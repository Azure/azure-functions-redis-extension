using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache.Tests.Unit
{
    public class AzureRedisCacheTriggerProviderTests
    {
        /// <summary>
        /// Test naming format : MethodName_StateUnderTest_ExpectedBehaviour
        /// </summary>

        // private static readonly Mock<IConfiguration> config = new Mock<IConfiguration>();

        readonly static IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "CacheConnection", "testing" }
        }).Build();

        readonly AzureRedisCacheTriggerProvider triggerProvider = new AzureRedisCacheTriggerProvider(config);

        [Fact]  
        public void ResolveConnectionString_ValidConnectionString_ReturnsResolvedConnectionString()
        {
            // arrange
            AzureRedisCacheTriggerAttribute unresolvedAzureRedisCacheTriggerAttribute = new AzureRedisCacheTriggerAttribute
            {
                CacheConnection = "%testing%"
            };
            string expected = "testing";

            // act
            string res = triggerProvider.ResolveConnectionString(unresolvedAzureRedisCacheTriggerAttribute);
           
            // assert
            Assert.Equal(expected , res);
        }

        //[Fact]
        //public void ResolveConnectionString_EmptyConnectionString_ThrowsArgumentNullException()
        //{
            // arrange
            //unresolvedAzureRedisCacheTriggerAttribute.CacheConnection = null;

            // act

            // assert

        //}

        //[Fact]
        //public void ResolveChannelName_ValidChannelName_ReturnsResolvedChannelName()
        //{
            // arrange
            //unresolvedAzureRedisCacheTriggerAttribute.ChannelName = "%hvhgcjh%";
            // act

            // assert

        //}

        //[Fact]
        //public void ResolveChannelName_EmptyChannelName_ThrowsArgumentNullException()
        //{
            // arrange
            //unresolvedAzureRedisCacheTriggerAttribute.ChannelName = null;
            // act

            // assert

        //}
    }
}
