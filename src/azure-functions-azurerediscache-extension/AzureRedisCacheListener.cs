using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    ///<summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    ///</summary>
    public class AzureRedisCacheListener : IListener
    {
        ITriggeredFunctionExecutor _executor;
        string _cacheConnectionString;
        string _channelName;
        bool _isKeySpaceNotificationsEnabled;
        Lazy<ConnectionMultiplexer> multiplexer;

        public AzureRedisCacheListener(string cacheConnection, string channelName, bool isKeySpaceNotificationsEnabled, ITriggeredFunctionExecutor executor = null)
        {
            _executor = executor;
            _cacheConnectionString = cacheConnection;
            _channelName = channelName;
            _isKeySpaceNotificationsEnabled = isKeySpaceNotificationsEnabled;

        }

        //Basic state machine for monitoring subscriber state:
        public enum SubscriberIsRunning {
            keySpaceNotifications = 0,
            pubSub = 1,
            Neither = 2,
        }
        SubscriberIsRunning subscriberCurrentState = SubscriberIsRunning.Neither;

        //Methods for redis cache connection management:
        string connectionString = "";
        public ConnectionMultiplexer RedisConnectionMultiplexer { get { return multiplexer.Value; } }

        ///<summary>
        ///Creates redis cache multiplexer connection.
        ///</summary>
        public void InitializeConnectionString(string cacheConnectionString)
        {
            multiplexer = CreateMultiplexer();
            if (string.IsNullOrWhiteSpace(cacheConnectionString))
                throw new ArgumentNullException(nameof(cacheConnectionString));

            connectionString = cacheConnectionString;
        }

        private Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        ///<summary>
        ///Closes redis cache multiplexer connection.
        ///</summary>
        public void CloseMultiplexer(ConnectionMultiplexer existingMultiplexer)
        {
            if (existingMultiplexer != null)
            {
                try
                {
                    existingMultiplexer.Close();
                }
                catch (Exception)
                {
                    throw new System.ArgumentException("Failed To Close Connection Multplexer.");
                }
            }
        }
        ///<summary>
        ///Process message from channel by building a AzureRedisCacheMessageModel & triggering the function.
        ///</summary>
        private async Task ProcessMessageAsync(CancellationToken cancellationtoken, string message = null, string notificationType = null, string key = null)
        {
            var callBack = new AzureRedisCacheMessageModel();
            callBack.Channel = _channelName;
            callBack.Message = message;
            callBack.KeySpaceNotification = notificationType;
            callBack.Key = key;

            FunctionResult result = await _executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = callBack }, cancellationtoken);
        }

        ///<summary>
        ///Enables basic redis pub/sub.
        ///</summary>
        public void EnablePubSub(ISubscriber subscriber, CancellationToken cancellationToken)
        {
            subscriberCurrentState = SubscriberIsRunning.pubSub;
            subscriber.Subscribe($"{_channelName}").OnMessage(async (msg) =>
           {
               //In pub/sub mode only AzureRedisCache.Message and AzureRedisCache.Channel are used.
               await ProcessMessageAsync(cancellationToken, msg.Message);
           });
        }

        ///<summary>
        ///Enables key space notifications on the channel provided in local.settings.json.
        ///</summary>
        public void EnableKeyspaceNotifications(ISubscriber subscriber, CancellationToken cancellationToken)
        {
            subscriberCurrentState = SubscriberIsRunning.keySpaceNotifications;
            subscriber.Subscribe($"{_channelName}", async (channel, notificationType) =>
            {
                string key = GetKey(channel);

                //In key space notificaitons mode only AzureRedisCache.Key, AzureRedisCache.KeyspaceNotifications and AzureRedisCache.Channel are used.
                await ProcessMessageAsync(cancellationToken, null, notificationType, key);
            });

            string GetKey(string channel)
            {
                var index = channel.IndexOf(':');
                if (index >= 0 && index < channel.Length - 1)
                    return channel.Substring(index + 1);

                //we didn't find the delimeter, so just return the whole thing
                return channel;
            }
        }

        ///<summary>
        ///Executes enabled functions, primary listener method.
        ///</summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitializeConnectionString(_cacheConnectionString);
            var subscriber = RedisConnectionMultiplexer.GetSubscriber();
            if (RedisConnectionMultiplexer.IsConnected)
            {
                Task.Run(() =>
                {
                    if (_isKeySpaceNotificationsEnabled)
                    {
                        EnableKeyspaceNotifications(subscriber, cancellationToken);
                    }
                    else {
                        EnablePubSub(subscriber, cancellationToken);
                    }
                });
            }
            else
            {
                throw new System.ArgumentException("Failed To Connect To Cache.");
            }
            return Task.CompletedTask;
        }

        ///<summary>
        ///Triggers disconnect from cache when cancellation token is invoked.
        ///</summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            subscriberCurrentState = SubscriberIsRunning.Neither;
            CloseMultiplexer(RedisConnectionMultiplexer);
            return Task.CompletedTask;
        }

        public void Cancel()
        {
            subscriberCurrentState = SubscriberIsRunning.Neither;
            CloseMultiplexer(RedisConnectionMultiplexer);
        }

        public void Dispose()
        {
            subscriberCurrentState = SubscriberIsRunning.Neither;
            CloseMultiplexer(RedisConnectionMultiplexer);
        }

        //Adding unit test infrastructure...
        public ConnectionMultiplexer getConnectionMultiplexer()
        {
            return RedisConnectionMultiplexer;
        }
        public SubscriberIsRunning getSubscriberState() {
            return subscriberCurrentState;
        }
    }
}
