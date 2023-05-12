
# Redis Extension for Azure Functions

## Introduction
This repository contains the triggers and bindings to use in your [Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-get-started) and [WebJobs](https://learn.microsoft.com/azure/app-service/webjobs-sdk-how-to).
There are three triggers in the Azure Functions Redis Extension:
- `RedisPubSubTrigger` triggers on [Redis pubsub messages](https://redis.io/docs/manual/pubsub/)
- `RedisListTrigger` triggers on [Redis lists](https://redis.io/docs/data-types/lists/)
- `RedisStreamTrigger` triggers on [Redis streams](https://redis.io/docs/data-types/streams/)

## Getting Started
1. [Set up an Azure Cache for Redis instance](https://learn.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis) or [install Redis locally](https://redis.io/download/).
1. Install the [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local).
1. Create a function project for .NET:
    ```cmd
    mkdir RedisFunctions
    cd RedisFunctions
    func init --worker-runtime dotnet
    ```
1. Install the Redis Extension using `dotnet add package Microsoft.Azure.WebJobs.Extensions.Redis --prerelease`.
For private preview, these are the following steps to add the nuget package to your project:
   1. Create a `NuGet.Config` file in the project folder (`RedisFunctions` in the above step):
      ```
      <?xml version="1.0" encoding="utf-8"?>
      <configuration>
        <packageSources>
          <add key="local-packages" value="./local-packages" />
        </packageSources>
      </configuration>
      ```
   1. Add the following line to the `<PropertyGroup>` section of the csproj.
      ```
      <RestoreSources>$(RestoreSources);./local-packages;https://api.nuget.org/v3/index.json</RestoreSources>
      ```
   1. Create a folder `local-packages` within the project folder, and download the latest NuGet package from [GitHub Releases](https://github.com/Azure/azure-functions-redis-extension/releases) to this `local-packages` folder.
   1. Install the package:
      ```
      dotnet add package Microsoft.Azure.WebJobs.Extensions.Redis --prerelease
      dotnet restore
      ```

## Usage
### `RedisPubSubTrigger`
The `RedisPubSubTrigger` subscribes to a specific channel or channel pattern and surfaces messages received on those channels to the function.

> **Warning**
> This trigger is not fully supported on a [Consumption plan](https://learn.microsoft.com/azure/azure-functions/consumption-plan) because Redis PubSub requires clients to always be actively listening to receive all messages.
> For consumption plans, there is a chance your function may miss certain messages published to the channel.

> **Note**
> In general, functions with this the `RedisPubSubTrigger` should not be scaled out to multiple instances.
> Each functions instance trigger on each message from the channel, resulting in duplicate processing.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
  - First attempts to resolve the connection string from the "ConnectionStrings" settings, and if not there, will look through the other appsettings for the string.
- `Channel`: name of the pubsub channel that the trigger should listen to.
  - Supports channel patterns.
  - This field can be resolved using `INameResolver`.

#### Avaiable Output Types
- [`StackExchange.Redis.ChannelMessage`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/ChannelMessageQueue.cs): The value returned by `StackExchange.Redis`.
- [`StackExchange.Redis.RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs),`string`,`byte[]`,`ReadOnlyMemory<byte>`: The message from the channel.
- `Custom`: The trigger uses Json.NET serialization to map the message from the channel from a `string` into a custom type.

#### Sample
The following sample listens to the channel `pubsubTest`. More samples can be found in the [samples](samples/RedisSamples.cs) or in the [integration tests](test/Integration/RedisPubSubTriggerTestFunctions.cs).
```c#
[FunctionName(nameof(PubSubTrigger))]
public static void PubSubTrigger(
    [RedisPubSubTrigger("redisConnectionStringSetting", "pubsubTest")] string message,
    ILogger logger)
{
    logger.LogInformation($"The message broadcast to channel pubsubTest: '{message}'");
}
```


### `RedisListTrigger`
The `RedisListTrigger` pops elements from a list and surfaces those elements to the function. The trigger polls Redis at a configurable fixed interval, and uses [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/)/[`LMPOP`](https://redis.io/commands/lmpop/) to pop elements from the lists.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Key`: Key to read from.
  - This field can be resolved using `INameResolver`.
- (optional) `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: 1000
- (optional) `MessagesPerWorker`: How many messages each functions worker "should" process. Used to determine how many workers the function should scale to.
  - Default: 100
- (optional) `Count`: Number of elements to pull from Redis at one time.
  - Default: 10
  - Only supported on Redis 6.2+ using the `COUNT` argument in [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/).
- (optional) `ListPopFromBeginning`: determines whether to pop elements from the beginning using [`LPOP`](https://redis.io/commands/lpop/) or to pop elements from the end using [`RPOP`](https://redis.io/commands/rpop/).
  - Default: true

#### Avaiable Output Types
- [`StackExchange.Redis.RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs),`string`,`byte[]`,`ReadOnlyMemory<byte>`: The entry from the list.
- `Custom`: The trigger uses Json.NET serialization to map the entry from the list from a `string` into a custom type.

#### Sample
The following sample polls the key `listTest` at a Redis instance defined in _local.settings.json_ at the key `redisConnectionStringSetting`.
```c#
[FunctionName(nameof(ListsTrigger))]
public static void ListsTrigger(
    [RedisListTrigger("redisConnectionStringSetting", "listTest")] string entry,
    ILogger logger)
{
    logger.LogInformation($"The entry pushed to the list listTest: '{entry}'");
}
```

### `RedisStreamTrigger`
The `RedisStreamTrigger` reads elements from a stream and surfaces those elements to the function.
The trigger polls Redis at a configurable fixed interval, and uses [`XREADGROUP`](https://redis.io/commands/xreadgroup/) to read elements from the stream.
The consumer group for all function workers will be the ID of the function (eg `Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisSamples.StreamTrigger` for the [StreamTrigger sample](samples/RedisSamples.cs)).
Each functions worker creates a new random GUID to use as its consumer name within the group to ensure that scaled out instances of the function will not read the same messages from the stream.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Key`: Key to read from.
  - This field can be resolved using `INameResolver`.
- (optional) `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: 1000
- (optional) `MessagesPerWorker`: How many messages each functions worker "should" process. Used to determine how many workers the function should scale to.
  - Default: 100
- (optional) `Count`: Number of elements to pull from Redis at one time.
  - Default: 10
- (optional) `DeleteAfterProcess`: If the listener will delete the stream entries after the function runs.
  - Default: false

#### Avaiable Output Types
- [`StackExchange.Redis.StreamEntry`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs): The value returned by `StackExchange.Redis`.
- `NameValueEntry[]`: The values contained within the entry as the underlying `StackExchange.Redis` type.
- `Dictionary<string, string>`: The values contained within the entry as a Dictionary.
- `string`, `byte[]`, `ReadOnlyMemory<byte>`: The stream entry serialized as JSON (UTF-8 encoded for byte types) in the following format:
    ```
    {"Id":"1658354934941-0","Values":{"field1":"value1","field2":"value2","field3":"value3"}}
    ```
- `Custom`: The trigger uses Json.NET serialization to map the values contained within the entry into the custom type.
  This is done by first turning the values within the stream into a Dictionary, and then deserializing that Dictionary into the custom type.

#### Sample
The following sample polls the key `streamTest` at a Redis instance defined in _local.settings.json_ at the key `redisConnectionStringSetting`.
More samples can be found in the [samples](samples/RedisSamples.cs) or in the [integration tests](test/Integration/RedisStreamTriggerTestFunctions.cs).
```c#
[FunctionName(nameof(StreamsTrigger))]
public static void StreamsTrigger(
    [RedisStreamTrigger("redisConnectionStringSetting", "streamTest")] string entry,
    ILogger logger)
{
    logger.LogInformation(entry);
}
```

## Known Issues
- The `RedisPubSubTrigger` is not capable of listening to [keyspace notifications](https://redis.io/docs/manual/keyspace-notifications/) on clustered caches.

## Contributing
This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
