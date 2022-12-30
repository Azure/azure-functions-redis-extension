using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisListsListener : RedisPollingListenerBase
    {
        internal bool listPopFromBeginning;

        public RedisListsListener(string connectionString, int pollingInterval, string keys, int count, bool listPopFromBeginning, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, keys, count, executor)
        {
            this.listPopFromBeginning = listPopFromBeginning;
        }

        public override async Task<bool> PollAsync(CancellationToken cancellationToken)
        {
            ListPopResult result;
            if (listPopFromBeginning)
            {
                result = await multiplexer.GetDatabase().ListLeftPopAsync(keys, count);
            }
            else
            {
                result = await multiplexer.GetDatabase().ListRightPopAsync(keys, count);
            }

            if (result.IsNull)
            {
                return true;
            }

            var triggerValue = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.List,
                Trigger = result.Key,
                Message = result.Values.ToStringArray()
            };
            
            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
            return false;
        }
    }
}
