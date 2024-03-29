﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisListTrigger
{
    public class CustomTypeListTrigger
    {
        private readonly ILogger<CustomTypeListTrigger> logger;

        public CustomTypeListTrigger(ILogger<CustomTypeListTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(CustomTypeListTrigger))]
        public void Run(
            [RedisListTrigger(Common.connectionString, "listKey")] Common.CustomType entry)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}