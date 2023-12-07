using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisPubSubTriggerTestFunctions
    {
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
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleChannels))]
        public static void MultipleChannels(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllChannels))]
        public static void AllChannels(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, allChannels)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleKey))]
        public static void SingleKey(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyspaceChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(MultipleKeys))]
        public static void MultipleKeys(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyspaceMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllKeys))]
        public static void AllKeys(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyspaceChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleEvent))]
        public static void SingleEvent(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyeventChannelSet)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(AllEvents))]
        public static void AllEvents(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, keyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_ChannelMessage))]
        public static void SingleChannel_ChannelMessage(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_RedisValue))]
        public static void SingleChannel_RedisValue(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] RedisValue message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_String))]
        public static void SingleChannel_String(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(ByteArray_SingleChannel))]
        public static void ByteArray_SingleChannel(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] byte[] message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }

        [FunctionName(nameof(SingleChannel_CustomType))]
        public static void SingleChannel_CustomType(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, pubsubChannel)] CustomType message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
