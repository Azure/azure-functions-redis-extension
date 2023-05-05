using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests
{
    public static class RedisPubSubTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string format = "triggerValue:{0}";
        public const string pubsubChannel = "testChannel";
        public const string pubsubMultiple = "testChannel*";
        public const string keyspaceChannel = "__keyspace@0__:testKey";
        public const string keyspaceMultiple = "__keyspace@0__:testKey*";
        public const string keyeventChannelSet = "__keyevent@0__:set";
        public const string keyeventChannelAll = "__keyevent@0__:*";
        public const string keyspaceChannelAll = "__keyspace@0__:*";
        public const string allChannels = "*";

        [FunctionName(nameof(SingleChannel))]
        public static void SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleChannels))]
        public static void MultipleChannels(
            [RedisPubSubTrigger(localhostSetting, pubsubMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllChannels))]
        public static void AllChannels(
            [RedisPubSubTrigger(localhostSetting, allChannels)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleKey))]
        public static void SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleKeys))]
        public static void MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllKeys))]
        public static void AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleEvent))]
        public static void SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelSet)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllEvents))]
        public static void AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_ChannelMessage))]
        public static void SingleChannel_ChannelMessage(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_RedisValue))]
        public static void SingleChannel_RedisValue(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisValue message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_String))]
        public static void SingleChannel_String(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(ByteArray_SingleChannel))]
        public static void ByteArray_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] byte[] message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_CustomType))]
        public static void SingleChannel_CustomType(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] CustomType message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
