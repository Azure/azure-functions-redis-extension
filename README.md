
# Redis Extension for Azure Functions

## Introduction
This repository contains the triggers and bindings to use in your [Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-get-started) and [WebJobs](https://learn.microsoft.com/azure/app-service/webjobs-sdk-how-to).
There are three triggers in this extension:
- `RedisPubSubTrigger` triggers on [Redis pub/sub messages](https://redis.io/docs/manual/pubsub/)
- `RedisListTrigger` triggers on [Redis list entries](https://redis.io/docs/data-types/lists/)
- `RedisStreamTrigger` triggers on [Redis stream entries](https://redis.io/docs/data-types/streams/)

## Getting Started
See the following docs for details on setup and samples of the Redis triggers:
- [C# (in-process)](docs/SetupGuide_Dotnet.md)
- [C# (out-of-process)](docs/SetupGuide_DotnetIsolated.md)
- [Java](docs/SetupGuide_Java.md)
- [Python](docs/SetupGuide_Python.md)
- [JavaScript](docs/SetupGuide_JavaScript.md)
- [PowerShell](docs/SetupGuide_PowerShell.md)

## Usage
### `RedisPubSubTrigger`
The `RedisPubSubTrigger` subscribes to a Redis pub/sub channel and surfaces messages received to the function.

> **Note**
> This trigger is only available on the [Premium plan](https://learn.microsoft.com/azure/azure-functions/premium-plan) and [Dedicated plan](https://learn.microsoft.com/azure/azure-functions/dedicated-plan) because Redis pub/sub requires clients to always be actively listening to receive all messages.
> There is a chance your function may miss messages on a consumption plan.

> **Note**
> Functions with this trigger should not be scaled out to multiple instances.
> Each instance will trigger on each message from the channel, resulting in duplicate processing.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the Redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Channel`: pubsub channel that the trigger should listen to.
  - Supports channel patterns.
  - This field can be resolved using `INameResolver`.

#### Avaiable Output Types
- [`StackExchange.Redis.ChannelMessage`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/ChannelMessageQueue.cs): The value returned by `StackExchange.Redis`.
- [`StackExchange.Redis.RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs), `string`, `byte[]`, `ReadOnlyMemory<byte>`: The message from the channel.
- `Custom`: The trigger uses Json.NET serialization to map the message from the channel from a `string` into a custom type.

#### Sample
The following sample listens to the channel `pubsubTest`.
More samples can be found in the [samples](samples/dotnet/RedisSamples.cs) or in the [integration tests](test/dotnet/Integration//RedisPubSubTriggerTestFunctions.cs).
```c#
[FunctionName(nameof(PubSubTrigger))]
public static void PubSubTrigger(
    [RedisPubSubTrigger("Redis", "pubsubTest")] string message,
    ILogger logger)
{
    logger.LogInformation($"The message broadcast to channel pubsubTest: '{message}'");
}
```


### `RedisListTrigger`
The `RedisListTrigger` pops entries from a list and surfaces those entries to the function. The trigger polls Redis at a configurable fixed interval, and uses [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/) to pop entries from the lists.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the Redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Key`: Key to read from.
  - This field can be resolved using `INameResolver`.
- `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: `1000`
- `MessagesPerWorker`: How many messages each functions instance "should" process. Used to determine how many instances the function should scale to.
  - Default: `100`
- `Count`: Number of entries to pop from Redis at one time. These are processed in parallel.
  - Default: `10`
  - Only supported on Redis 6.2+ using the `COUNT` argument in [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/).
- `ListPopFromBeginning`: determines whether to pop entries from the beginning using [`LPOP`](https://redis.io/commands/lpop/) or to pop entries from the end using [`RPOP`](https://redis.io/commands/rpop/).
  - Default: `true`

#### Avaiable Output Types
- [`StackExchange.Redis.RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs), `string`, `byte[]`, `ReadOnlyMemory<byte>`: The entry from the list.
- `Custom`: The trigger uses Json.NET serialization to map the entry from the list from a `string` into a custom type.

#### Sample
The following sample polls the key `listTest`.
More samples can be found in the [samples](samples/dotnet/RedisSamples.cs) or in the [integration tests](test/dotnet/Integration/RedisListTriggerTestFunctions.cs.cs).
```c#
[FunctionName(nameof(ListsTrigger))]
public static void ListsTrigger(
    [RedisListTrigger("Redis", "listTest")] string entry,
    ILogger logger)
{
    logger.LogInformation($"The entry pushed to the list listTest: '{entry}'");
}
```

### `RedisStreamTrigger`
The `RedisStreamTrigger` reads entries from a stream and surfaces those entries to the function.
The trigger polls Redis at a configurable fixed interval, and uses [`XREADGROUP`](https://redis.io/commands/xreadgroup/) to read entries from the stream.
The consumer group for all function instances will be the ID of the function (eg `Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisSamples.StreamTrigger` for the [StreamTrigger sample](samples/dotnet/RedisSamples.cs)).
Each functions instance creates a new random GUID to use as its consumer name within the group to ensure that scaled out instances of the function will not read the same messages from the stream.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the Redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Key`: Key to read from.
  - This field can be resolved using `INameResolver`.
- `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: `1000`
- `MessagesPerWorker`: How many messages each functions instance "should" process. Used to determine how many instances the function should scale to.
  - Default: `100`
- `Count`: Number of entries to read from Redis at one time. These are processed in parallel.
  - Default: `10`
- `DeleteAfterProcess`: Whether to delete the stream entries after the function has run.
  - Default: `false`

#### Avaiable Output Types
- [`StackExchange.Redis.StreamEntry`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs): The value returned by `StackExchange.Redis`.
- `StackExchange.Redis.NameValueEntry[]`, `Dictionary<string, string>`: The values contained within the entry.
- `string`, `byte[]`, `ReadOnlyMemory<byte>`: The stream entry serialized as JSON (UTF-8 encoded for byte types) in the following format:
    ```
    {"Id":"1658354934941-0","Values":{"field1":"value1","field2":"value2","field3":"value3"}}
    ```
- `Custom`: The trigger uses Json.NET serialization to map the values contained within the entry from a `string` into a custom type.

#### Sample
The following sample polls the key `streamTest`.
More samples can be found in the [samples](samples/dotnet/RedisSamples.cs) or in the [integration tests](test/dotnet/Integration/RedisStreamTriggerTestFunctions.cs).
```c#
[FunctionName(nameof(StreamsTrigger))]
public static void StreamsTrigger(
    [RedisStreamTrigger("Redis", "streamTest")] string entry,
    ILogger logger)
{
    logger.LogInformation($"The entry pushed to the list listTest: '{entry}'");
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
