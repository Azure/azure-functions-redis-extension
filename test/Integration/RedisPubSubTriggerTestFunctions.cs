using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
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
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleChannels))]
        public static void MultipleChannels(
            [RedisPubSubTrigger(localhostSetting, pubsubMultiple)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllChannels))]
        public static void AllChannels(
            [RedisPubSubTrigger(localhostSetting, allChannels)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleKey))]
        public static void SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleKeys))]
        public static void MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceMultiple)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllKeys))]
        public static void AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleEvent))]
        public static void SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelSet)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllEvents))]
        public static void AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(ChannelMessage_SingleChannel))]
        public static void ChannelMessage_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(RedisValue_SingleChannel))]
        public static void RedisValue_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisValue message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(String_SingleChannel))]
        public static void String_SingleChannel(
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

        [FunctionName(nameof(CustomChannelMessage_SingleChannel))]
        public static void CustomChannelMessage_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] CustomChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
