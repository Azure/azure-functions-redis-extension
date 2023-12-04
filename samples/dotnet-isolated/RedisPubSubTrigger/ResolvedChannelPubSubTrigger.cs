﻿using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    internal class ResolvedChannelPubSubTrigger
    {
        private readonly ILogger<ResolvedChannelPubSubTrigger> logger;

        public ResolvedChannelPubSubTrigger(ILogger<ResolvedChannelPubSubTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(ResolvedChannelPubSubTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "%pubsubChannel%")] string message)
        {
            logger.LogInformation(message);
        }
    }
}