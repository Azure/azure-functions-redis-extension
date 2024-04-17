using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisStreamTriggerScaleMonitor : RedisPollingTriggerBaseScaleMonitor
    {
        public RedisStreamTriggerScaleMonitor(
            string name,
            IConfiguration configuration,
            AzureComponentFactory azureComponentFactory,
            string connection,
            int maxBatchSize,
            string key)
            : base(name, configuration, azureComponentFactory, connection, maxBatchSize, key)
        {
            this.Descriptor = new ScaleMonitorDescriptor(name, RedisScalerProvider.GetFunctionScalerId(name, RedisUtilities.RedisStreamTrigger, key));
            this.TargetScalerDescriptor = new TargetScalerDescriptor(RedisScalerProvider.GetFunctionScalerId(name, RedisUtilities.RedisStreamTrigger, key));
        }

        public override async Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            try
            {
                IConnectionMultiplexer multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, azureComponentFactory, connection, nameof(RedisStreamTriggerScaleMonitor));
                long streamLength = await multiplexer.GetDatabase().StreamLengthAsync(key);

                StreamGroupInfo[] groups = multiplexer.GetDatabase().StreamGroupInfo(key);
                if (!groups.Any(g => g.Name == name))
                {
                    // group doesn't exist, assume all entries in stream have not been processed
                    return new RedisPollingTriggerBaseMetrics
                    {
                        Remaining = streamLength,
                        Timestamp = DateTime.UtcNow,
                    };
                }

                StreamGroupInfo group = groups.First(g => g.Name == name);
                Version serverVersion = multiplexer.GetServers()[0].Version;
                if (serverVersion >= RedisUtilities.Version70 && group.Lag.HasValue)
                {
                    // Redis 7: Scaler gets number of remaining entries for the consumer group from XINFO GROUPS.
                    return new RedisPollingTriggerBaseMetrics
                    {
                        Remaining = group.Lag.Value,
                        Timestamp = DateTime.UtcNow,
                    };
                }

                // Redis 6/6.2 does not have an internal counter for remaining entries for the consumer group
                // Estimate remaining entries using the time field from the entry ID (assuming ID="time-counter")
                StreamInfo info = await multiplexer.GetDatabase().StreamInfoAsync(key);
                string firstId = (string)info.FirstEntry.Id;
                string lastId = (string)info.LastEntry.Id;
                string lastDeliveredId = group.LastDeliveredId ?? firstId;
                string[] firstIdSplit = firstId.Split('-');
                string[] lastIdSplit = lastId.Split('-');
                string[] lastDeliveredIdSplit = lastDeliveredId.Split('-');
                ulong firstTimestamp = ulong.Parse(firstIdSplit[0]);
                ulong lastTimestamp = ulong.Parse(lastIdSplit[0]);
                ulong lastDeliveredTimestamp = ulong.Parse(lastDeliveredIdSplit[0]);

                long estimatedRemaining = 0;
                if (lastTimestamp == lastDeliveredTimestamp)
                {
                    // If timestamp is the same, return counter difference
                    ulong lastCounter = ulong.Parse(lastIdSplit[1]);
                    ulong lastDelieveredCoutner = ulong.Parse(lastDeliveredIdSplit[1]);
                    estimatedRemaining = Math.Min(streamLength, (long)Math.Max(0, lastCounter - lastDelieveredCoutner));
                }
                else
                {
                    // Assume percentage of time processsed as percentage of entries processed
                    double timeRemaining = Math.Max(1, lastTimestamp - lastDeliveredTimestamp);
                    double timeTotal = Math.Max(1, lastTimestamp - firstTimestamp);
                    double percentageRemaining = timeRemaining / timeTotal;
                    estimatedRemaining = (long)Math.Min(streamLength, Math.Max(0, percentageRemaining * streamLength));
                }

                return new RedisPollingTriggerBaseMetrics
                {
                    Remaining = estimatedRemaining,
                    Timestamp = DateTime.UtcNow,
                };
            }
            catch (ObjectDisposedException)
            {
                // If multiplexer is disposed, then listener has already been stopped.
            }
            catch (RedisException)
            {
                // If the key is not a stream, there should be no length.
            }

            return new RedisPollingTriggerBaseMetrics
            {
                Remaining = 0,
                Timestamp = DateTime.UtcNow,
            };
        }
    }
}
