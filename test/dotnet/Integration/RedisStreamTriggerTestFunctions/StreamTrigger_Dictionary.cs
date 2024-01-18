using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Dictionary
    {
        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(StreamTrigger_Dictionary), IntegrationTestHelpers.PollingIntervalShort)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
