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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="handler"></param>
        void Subscribe(string channel, Func<ChannelMessage, Task> handler);
        
        /// <summary>
        /// 
        /// </summary>
        void Close();
    }
}
