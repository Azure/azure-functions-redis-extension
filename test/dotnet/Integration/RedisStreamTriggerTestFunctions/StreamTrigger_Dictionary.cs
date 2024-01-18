using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Dictionary
    {
        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Dictionary), IntegrationTestHelpers.pollingIntervalShort)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
