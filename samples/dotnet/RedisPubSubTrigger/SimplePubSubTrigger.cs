﻿using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class SimplePubSubTrigger
    {
        [FunctionName(nameof(SimplePubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "pubsubTest")] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(message.Message);
        }
    }
}
