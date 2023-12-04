﻿using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    internal class KeyeventTrigger
    {
        private readonly ILogger<KeyeventTrigger> logger;

        public KeyeventTrigger(ILogger<KeyeventTrigger> logger)
        {
            this.logger = logger;
        }
        
        [Function(nameof(KeyeventTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "__keyevent@0__:del")] string message)
        {
            logger.LogInformation($"Key '{message}' deleted.");
        }
    }
}