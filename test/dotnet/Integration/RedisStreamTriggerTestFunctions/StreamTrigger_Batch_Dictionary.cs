using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Batch_Dictionary
    {
        [FunctionName(nameof(StreamTrigger_Batch_Dictionary))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionString, nameof(StreamTrigger_Batch_Dictionary), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] Dictionary<string, string>[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
