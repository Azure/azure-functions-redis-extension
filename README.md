
# Redis Extension for Azure Functions

## Introduction
This repository contains the bindings to use in your [Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-get-started) and [WebJobs](https://learn.microsoft.com/azure/app-service/webjobs-sdk-how-to).

There are five bindings in this extension:
- `RedisPubSubTrigger` trigger binding for [Redis pub/sub messages](https://redis.io/docs/manual/pubsub/)
- `RedisListTrigger` trigger binding for [Redis list entries](https://redis.io/docs/data-types/lists/)
- `RedisStreamTrigger` trigger binding for [Redis stream entries](https://redis.io/docs/data-types/streams/)
- `RedisInput` input binding for reading from Redis
- `RedisOutput` output binding for writing to Redis

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

> **Note**
> This trigger is not capable of listening to keyspace notifications on clustered caches.

#### Inputs
- `Connection`: App setting name that contains Redis connection information.
- `Channel`: Redis pubsub channel that the trigger will listen to.
- `Pattern`: If the channel is a pattern.
  - Default: `false`

#### Available Parameter Types
- Types exclusive to `dotnet in-process`
  - [`ChannelMessage`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/ChannelMessageQueue.cs): The value returned by `StackExchange.Redis`.
  - [`RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs): The message from the channel [`ChannelMessage.Message`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/ChannelMessageQueue.cs)
- Types available to all (`dotnet in-process`, `dotnet out-of-process`, `python`, `node`, `java`, `powershell`, etc)
  - `string`, `byte[]`/`ReadOnlyMemory<byte>`: The channel message serialized as JSON (UTF-8 encoded for byte types) in the following format:
    ```json
    {
      "SubscriptionChannel":"__keyspace@0__:*",
      "Channel":"__keyspace@0__:mykey",
      "Message":"set"
    }
    ```
  - `Custom`: The trigger uses Json.NET serialization to map `string` value into the given custom type.

#### Sample
The following sample listens to the channel `pubsubTest`.
```c#
[FunctionName(nameof(PubSubTrigger))]
public static void PubSubTrigger(
    [RedisPubSubTrigger("Redis", "pubsubTest")] string message,
    ILogger logger)
{
    logger.LogInformation($"The message broadcast to channel 'pubsubTest': '{message}'");
}
```

### `RedisListTrigger`
The `RedisListTrigger` pops entries from a list and surfaces those entries to the function. The trigger polls Redis at a configurable fixed interval, and uses [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/) to pop entries from the lists.

#### Inputs
- `Connection`: App setting name that contains Redis connection information.
- `Key`: Key to read from.
- `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: `1000`
- `MaxBatchSize`: Number of entries to pop from the Redis list at one time.
These are usually processed in parallel, with each entry from the list triggering its own function invocation.
If a `dotnet in-process` function defines the parameter type as [`RedisValue[]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs), `string[]`, or `byte[][]`, there will only be one function invocation with all entries returned as this array type.
  - Only supported on Redis 6.2+ using the `COUNT` argument in [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/).
  - Default: `16`
- `ListDirection`: The direction to pop elements from the list: `LEFT` using [`LPOP`](https://redis.io/commands/lpop/) or `RIGHT` using [`RPOP`](https://redis.io/commands/rpop/).
  - Allowed Values: `LEFT`, `RIGHT`
  - Default: `LEFT`

#### Available Parameter Types
- Types exclusive to `dotnet in-process`
  - [`RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs): The entry from the list.
  - Single-invocation batch types: [`RedisValue[]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs), `string[]`, `byte[][]` -  The full batch of list entries given to the function in one invocation.
- Types available to all (`dotnet in-process`, `dotnet out-of-process`, `python`, `node`, `java`, `powershell`, etc)
  - `string`, `byte[]`/`ReadOnlyMemory<byte>`: The entry from the list.
  - `Custom`: The trigger uses Json.NET serialization to map `string` value into the given custom type.

#### Sample
The following sample polls the key `listTest`.
```c#
[FunctionName(nameof(ListsTrigger))]
public static void ListsTrigger(
    [RedisListTrigger("Redis", "listTest")] string entry,
    ILogger logger)
{
    logger.LogInformation($"The entry pushed to the list 'listTest': '{entry}'");
}
```

### `RedisStreamTrigger`
The `RedisStreamTrigger` reads entries from a stream and surfaces those entries to the function.
The trigger polls Redis at a configurable fixed interval, and uses [`XREADGROUP`](https://redis.io/commands/xreadgroup/) to read entries from the stream.
The consumer group for all instances of a function will be the name of the function (e.g. `SimpleStreamTrigger` for [this sample](samples/dotnet/RedisStreamTrigger/SimpleStreamTrigger.cs)).
Each functions instance will use the `WEBSITE_INSTANCE_ID` ([documented here](https://learn.microsoft.com/azure/app-service/reference-app-settings?tabs=kudu%2Cdotnet#scaling)) or generate a random GUID to use as its consumer name within the group to ensure that scaled out instances of the function will not read the same messages from the stream.

#### Inputs
- `Connection`: App setting name that contains Redis connection information.
- `Key`: Key to read from.
- `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: `1000`
- `MaxBatchSize`: Number of entries to read from Redis at one time.
These are usually processed in parallel, with each entry from the list triggering its own function invocation.
If a `dotnet in-process` function defines the parameter type as [`StreamEntry[]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs), [`NameValueEntry[][]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/NameValueEntry.cs), `Dictionary<string, string>[]`, `string[]`, or `byte[][]`, there will only be one function invocation with all entries returned as this array type.
  - Default: `16`

#### Available Parameter Types
- Types exclusive to `dotnet in-process`
  - [`StreamEntry`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs): The value returned by `StackExchange.Redis`.
  - [`NameValueEntry[]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/NameValueEntry.cs): The values contained within the entry ([`StreamEntry.Values`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs))
  - `Dictionary<string, string>`: The values contained within the entry ([`StreamEntry.Values`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs)) converted into a `Dictionary<string, string>`.
    - Single-invocation batch types: [`StreamEntry[]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs), [`NameValueEntry[][]`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/NameValueEntry.cs), `Dictionary<string, string>[]`, `string[]`, `byte[][]` -  The full batch of list entries given to the function in one invocation.
- Types available to all (`dotnet in-process`, `dotnet out-of-process`, `python`, `node`, `java`, `powershell`, etc)
  - `string`, `byte[]`, `ReadOnlyMemory<byte>`: The stream entry serialized as JSON (UTF-8 encoded for byte types) in the following format:
    ```json
    {
      "Id":"1658354934941-0",
      "Values":
      {
        "field1":"value1",
        "field2":"value2",
        "field3":"value3"
      }
    }
    ```
  - `Custom`: The trigger uses Json.NET serialization to map `string` value into the given custom type.

#### Sample
The following sample polls the key `streamTest`.
```c#
[FunctionName(nameof(StreamsTrigger))]
public static void StreamsTrigger(
    [RedisStreamTrigger("Redis", "streamTest")] string entry,
    ILogger logger)
{
    logger.LogInformation($"The entry pushed to the stream 'streamTest': '{entry}'");
}
```

### `RedisInput` Input Binding

#### Inputs
- `Connection`: App setting name that contains Redis connection information.
- `Command`: The redis-cli command to be executed on the cache with all arguments separated by spaces. (e.g. `"GET key"`, `"HGET key field"`)

> **Note**
> Not all commands are supported for this binding. At the moment, only read commands that return a single output are supported.
> The full list can be found [here](./src/Microsoft.Azure.WebJobs.Extensions.Redis/Bindings/RedisConverter.cs#L61)

#### Available Parameter Types
- [`RedisValue`](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/RedisValue.cs), `string`, `byte[]`, `ReadOnlyMemory<byte>`: The value returned by the command.
- `Custom`: The trigger uses Json.NET serialization to map the value returned by the command from a `string` into a custom type.

#### Sample
The following gets any key that was recently set.
```c#
[FunctionName(nameof(SetGetter))]
public static void SetGetter(
    [RedisPubSubTrigger("Redis", "__keyevent@0__:set")] string key,
    [Redis("Redis", "GET {Message}")] string value,
    ILogger logger)
{
    logger.LogInformation($"Key '{key}' was set to value '{value}'");
}
```

### `RedisOutput` Output Binding

#### Inputs
- `Connection`: App setting name that contains Redis connection information.
- `Command`: The Redis command to be executed on the cache without any arguments. (e.g. `"GET"`, `"HGET"`)

> **Note**
> All commands are supported for this binding.

#### Function Return type
- `string`: Space-delimited arguments for the redis command.

#### Sample
The following deletes any key that was recently set.
```c#
[FunctionName(nameof(SetDeleter))]
public static void SetDeleter(
    [RedisPubSubTrigger("Redis", "__keyevent@0__:set")] string key,
    [Redis("Redis", "DEL")] out string arguments,
    ILogger logger)
{
    logger.LogInformation($"Deleting recently SET key '{key}'");
    arguments = key;
}
```

## Connection Types
There are three types of connections that are allowed from an Azure Functions instane to a Redis Cache.
In the appsettings, this is how to configure each of the following types of client authentication, assuming the `Connection` was set to `"Redis"` in the function.
1. Connection String
    ```json
    "Redis": "<cacheName>.redis.cache.windows.net:6380,password=..."
    ```

1. System-Assigned Managed Identity
    ```json
    "Redis:redisHostName": "<cacheName>.redis.cache.windows.net",
    "Redis:principalId": "<principalId>"
    ```

1. User-Assigned Managed Identity
    ```json
    "Redis:redisHostName": "<cacheName>.redis.cache.windows.net",
    "Redis:principalId": "<principalId>",
    "Redis:clientId": "<clientId>"
    ```

1. Service Principal Secret
    > **Note**
    > Connections using Service Principal Secrets are only available during local development.
    ```json
    "Redis:redisHostName": "<cacheName>.redis.cache.windows.net",
    "Redis:principalId": "<principalId>",
    "Redis:clientId": "<clientId>"
    "Redis:tenantId": "<tenantId>"
    "Redis:clientSecret": "<clientSecret>"
    ```

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
