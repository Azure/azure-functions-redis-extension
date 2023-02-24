using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// 
        /// </summary>
        void Connect();

        void Subscribe(RedisChannel channel, Func<ChannelMessage, Task> handler);
        Task<RedisValue> ListLeftPopAsync(RedisKey key);
        Task<RedisValue[]> ListLeftPopAsync(RedisKey key, long count);
        Task<ListPopResult> ListLeftPopAsync(RedisKey[] keys, long count);
        Task<RedisValue> ListRightPopAsync(RedisKey key);
        Task<RedisValue[]> ListRightPopAsync(RedisKey key, long count);
        Task<ListPopResult> ListRightPopAsync(RedisKey[] keys, long count);
        Task<long> ListLengthAsync(RedisKey key);

        /// <summary>
        /// 
        /// </summary>
        void Close();
    }
}
