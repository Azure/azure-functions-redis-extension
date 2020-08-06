
# Azure Redis Cache Extension for Azure Functions

The Azure Functions Redis cache extension allows you to receive Redis pub/sub messages and Redis key space notification events from your Azure Redis cache all via the [Azure functions interface](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio).

To get started with developing with this extension, make sure you first [set up an Azure cache for Redis instance](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/quickstart-create-redis). Then you can go ahead and begin developing your functions in azure portal or visual studio with C#. 

NOTE: _While Azure function extensions have multi-language support, for the purposes of this demo I will be using C# & Visual Studio._

# Installation
> 1. Once you've downloaded/cloned the repo, go ahead and open the `src` directory. 
> 
> 2. In here you will see 3 folders, `azure-functions-azurerediscache-extension`, `azurerediscache-demo-function` & `tests`.
> 
> 3. For demo purposes the Azure function trigger in the `azurerediscache-demo-function` folder already has the `azure-functions-azurerediscache-extension` library linked as a _project reference_. 
> But if you don't which to use project references in your own project a NuGet package will also be available.
> 
> 4. All the trigger configuration code below is written in `azurerediscache-demo-function/AzureRedisCacheDemoFunction.cs`
> 
> 5. Once you open the solution & provide the trigger with the required parameters you should be able to run this Azure function locally.

# Trigger Parameter Configuration
When running locally, trigger input parameters can either be passed in literally ie. (`CacheConnection = "[EXAMPLE_CACHE_NAME].redis.cache.windows.net:6380, password=..."`) or (like in the example below) with a reference to a `local.settings.json` file, which looks like this:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "CacheConnection": "[EXAMPLE_CACHE_NAME].redis.cache.windows.net:6380, password=...",
    "ChannelName": "demochannel"
  }
}
```
And similarly to the example(s) below, when referring to keys in this `local.settings.json` file in your function trigger input parameter, you use the `%` symbol as a wrapper around your key name ie. (`ChannelName = %CacheConnection%`). 

NOTE: _The only trigger parameter that doesn't support _local.settings.json_ key fetching is `isKeySpaceNotificationsEnabled` as it is simply just a bool_.


## Trigger Parameter Properties

|Property Name|Description|Type|Example|Can be resolved from _local.settings.json_?
|--|--|--|--|--|
|CacheConnection|The connection string for the Azure cache for redis instance.|`string`|`[EXAMPLE_CACHE_NAME].redis.cache.windows.net:6380, password=..."`|yes|
|ChannelName|The name of the Redis channel you wish to subscribe to.|`string`| `demochannel` (or when using key space event notifications: `__keyspace@0__:*`)|yes|
|isKeySpaceNotificationsEnabled|A boolean value indicating whether or not you wish to enable key space event notifications.|`bool`| `true` (or `false`)|no|

# Pub-Sub Mode Configuration
See [AzureRedisCacheListener.cs](https://github.com/microsoft/Azure-functions-AzureRedisCache-Extension/src/azure-functions-azurerediscache-extension/AzureRedisCacheListener.cs) for additional context on _pub-sub_ mode implementation.

```C#
[FunctionName("AzureRedisCachePubSubDemo")]

public static void Run([AzureRedisCacheTrigger(CacheConnection = "%CacheConnection%", ChannelName = "%ChannelName%")]
	AzureRedisCacheMessageModel result, ILogger logger)
{
	logger.LogInformation($"Pub-Sub Message: {result.Message}");
}
```
The above sample function is triggered every time a new message is consumed from the Redis pub-sub channel provided via the `ChannelName` input parameter (further explained in the **trigger parameter properties** section of this document). 

To ensure pub-sub mode is enabled correctly, provide valid `CacheConnection` string and `ChannelName` string parameters. 

Note: _Trigger input parameters can be passed in literally or via the _local.settings.json_ configuration file, further discussed in the **trigger parameter configuration** section of this document_.

# Key Space Notifications Mode Configuration
See [AzureRedisCacheListener.cs](https://github.com/microsoft/Azure-functions-AzureRedisCache-Extension/src/azure-functions-azurerediscache-extension/AzureRedisCacheListener.cs) for additional context on _key space notifications_ mode implementation.

Before we jump into the code example for this we need to ensure that Redis key space notifications are enabled in our cache. To do this:

> 1. Head over to your [azure portal](https://ms.portal.azure.com/) and select your Azure cache for Redis resource.
> 2. Once you are on your caches portal page, head over to the side bar and click `Advanced Settings`.
> 3. Scroll down to `notify-keyspace-events` and set it to your desired [key space configuration](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-configure#keyspace-notifications-advanced-settings). 
> 4. Save your changes.
> 
	> Note: _If you wish to enable all possible key space notifications on your Redis cache, simply enter "`KEA`" as your desired configuration in the `notify-keyspace-events` field._

Now taking a look at implementing the key space event notifications function trigger...

```C#
[FunctionName("AzureRedisCacheKeySpaceNotificationsDemo")]

public static void Run( [AzureRedisCacheTrigger(CacheConnection = "%CacheConnection%", ChannelName = "%ChannelName%", IsKeySpaceNotificationsEnabled = true)] 
	AzureRedisCacheMessageModel result, ILogger logger)
{
	logger.LogInformation($"key {result.Key} had an event: {result.KeySpaceNotification}");
}
```
The above sample function is triggered every time a new [Redis key space event notification](https://redis.io/topics/notifications) is generated. The listener does this by subscribing to the dedicated key space event notifications channel, which _generally_ looks like this `__keyspace@<INT>__:*`. 

I say generally because the `@<INT>` in that channel name can be changed to whichever Redis db instance you want it to run on (usually db 0, so it would look like this `__keyspace@0__:*`). As for the `:*` portion of this channel name, it can be further customized [to only observe specific keys or key space events](https://redis.io/topics/notifications) (`:*` observes all changes made to all keys). 

This specialized channel name is provided to the function trigger via the `ChannelName` parameter. 

To ensure key space notifications mode is enabled correctly, please provide valid `CacheConnection` string and valid key space notification `ChannelName` string parameters, and ensure the `isKeySpaceNotificationsEnabled` boolean parameter is set to true.



Note: _Trigger input parameters can be passed in literally or via the _local.settings.json_ configuration file, further discussed in the **trigger parameter configuration** section of this document_.

# Trigger Output - `AzureRedisCacheMessageModel`
You may have noticed that when we log our output from the function trigger, we are referring to an object named 'result' of the type `AzureRedisCacheModel`. 

This object gets returned on every instance of function execution and is effectively the 'output' of the Azure function. Let us take a closer look at this object:

```C#
namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    public class AzureRedisCacheMessageModel
    {
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Key { get; set; }
        public string KeySpaceNotification { get; set; }

    }
}
```
In the interest of posterity, flexibility and overall simplicity of use, this object has 4 attributes `Channel`, `Message`, `Key` & `KeySpaceNotification`. Depending on what mode you're running this extension in, **pub-sub** or **key space notifications** the attributes available to you will vary.

### In pub-sub mode:
> The _only_ attributes available are `Channel` & `Message`. 
>Other attributes (`Key` & `KeySpaceNotification`) will be default set to *null*.
### In key space notifications mode:
> The _only_ attributes available are `Channel`, `Key` & `KeySpaceNotification`.
>The other attribute (`Message`) will be default set to *null*.

# Unit Testing
This Azure Function extension supports unit testing (via XUnit). No special requirements are needed for this other than you providing a valid _cache connection string_ and a _channel name_ via a local config file (`tests.local.settings.json` by default). Let us take a look at how a local config file is set up and how an included test file references it:

```json
{
  "ConnectionString": "<YOUR_CACHE_CONNECTION_STRING_HERE>",
  "ChannelName": "<YOUR_CHANNEL_NAME_HERE>"
}
```
This is the `tests.local.settings.json`  file which is included by default with the project. The config is quite straight forward, just provide a valid `ConnectionString` and `ChannelName`. If you wish to change the name/location of this file, remember to update the file name/location reference in `AzureRedisCacheListenerTests.cs`. That reference looks like this:
```C#
namespace XUnitFunctionTests
{
    public class AzureRedisCacheListenerTests
    {
        static TestCacheConfig cacheConfig = new TestCacheConfig("tests.local.settings.json");
        static string cacheConnectionString = cacheConfig.connectionString;
        static string channelName = cacheConfig.channelName;
```
Note: _If you wish to add additional keys to the local config JSON file you must add the according attributes to the [TestCacheConfig.cs](https://github.com/microsoft/Azure-functions-AzureRedisCache-Extension/src/Tests/TestCacheConfig.cs) class, as well as the private `jsonConfig` local sub-class (which is used for JSON deserialization)_.

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

### Creators
> Jaden Banson (https://github.com/jadenbanson). A very special thank you to:
>  - Lavanya Singampalli
>  - Alfan T.P
>  - Ankit Kumar