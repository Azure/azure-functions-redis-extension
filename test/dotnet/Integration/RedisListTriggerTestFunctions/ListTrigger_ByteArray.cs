using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_ByteArray
    {
        [FunctionName(nameof(ListTrigger_ByteArray))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(ListTrigger_ByteArray), IntegrationTestHelpers.PollingIntervalShort)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
