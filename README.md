
# Redis Extension for Azure Functions

## Introduction
This repository contains the triggers and bindings to use in your [Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-get-started) and [WebJobs](https://learn.microsoft.com/azure/app-service/webjobs-sdk-how-to).
There are three triggers in the Azure Functions Redis Extension:
- `RedisPubSubTrigger` triggers on [Redis pubsub messages](https://redis.io/docs/manual/pubsub/)
- `RedisListsTrigger` triggers on [Redis lists](https://redis.io/docs/data-types/lists/)
- `RedisStreamsTrigger` triggers on [Redis streams](https://redis.io/docs/data-types/streams/)

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
The `RedisPubSubTrigger` subscribes to a specific channel pattern using [`PSUBSCRIBE`](https://redis.io/commands/psubscribe/), and surfaces messages received on those channels to the function.

> **Warning**
> This trigger is not fully supported on a [Consumption plan](https://learn.microsoft.com/azure/azure-functions/consumption-plan) because Redis PubSub requires clients to always be actively listening to receive all messages.
> For consumption plans, there is a chance your function may miss certain messages published to the channel. Functions with this trigger shuld also not be scaled out.

> **Note**
> In general, functions with this the `RedisPubSubTrigger` should not be scaled out to multiple instances.
> Each instance will listen and process each pubsub message, resulting in duplicate processing.

#### Inputs
- `ConnectionString`: connection string to the redis cache (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Channel`: name of the pubsub channel that the trigger should listen to.

#### Sample
The following sample listens to the channel "channel" at a localhost Redis instance at "127.0.0.1:6379"
```c#
[FunctionName(nameof(PubSubTrigger))]
public static void PubSubTrigger(
    [RedisPubSubTrigger(ConnectionString = "127.0.0.1:6379", Channel = "channel")] RedisMessageModel model,
    ILogger logger)
{
    logger.LogInformation(JsonSerializer.Serialize(model));
}
```

### `RedisListsTrigger`
The `RedisListsTrigger` pops elements from a list and surfaces those elements to the function. The trigger polls Redis at a configurable fixed interval, and uses [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/)/[`LMPOP`](https://redis.io/commands/lmpop/) to pop elements from the lists.

Inputs:
- `ConnectionString`: connection string to the redis cache (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Keys`: Keys to read from, space-delimited.
  - Multiple keys only supported on Redis 7.0+ using [`LMPOP`](https://redis.io/commands/lmpop/).
  - Listens to only the first key given in the argument using [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/) on Redis versions less than 7.0.
- (optional) `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: 1000
- (optional) `MessagesPerWorker`: How many messages each functions worker "should" process. Used to determine how many workers the function should scale to.
  - Default: 100
- (optional) `BatchSize`: Number of elements to pull from Redis at one time.
  - Default: 10
  - Only supported on Redis 6.2+ using the `COUNT` argument in [`LPOP`](https://redis.io/commands/lpop/)/[`RPOP`](https://redis.io/commands/rpop/).
- (optional) `ListPopFromBeginning`: determines whether to pop elements from the beginning using [`LPOP`](https://redis.io/commands/lpop/) or to pop elements from the end using [`RPOP`](https://redis.io/commands/rpop/).
  - Default: true

#### Sample
The following sample polls the key "listTest" at a localhost Redis instance at "127.0.0.1:6379"
```c#
[FunctionName(nameof(ListsTrigger))]
public static void ListsTrigger(
    [RedisListsTrigger(ConnectionString = "127.0.0.1:6379", Keys = "listTest")] RedisMessageModel model,
    ILogger logger)
{
    logger.LogInformation(JsonSerializer.Serialize(model));
}
```

### `RedisStreamsTrigger`
The `RedisStreamsTrigger` pops elements from a stream and surfaces those elements to the function.
The trigger polls Redis at a configurable fixed interval, and uses [`XREADGROUP`](https://redis.io/commands/xreadgroup/) to read elements from the stream.
Each function creates a new random GUID to use as its consumer name within the group to ensure that scaled out instances of the function will not read the same messages from the stream.

#### Inputs
- `ConnectionStringSetting`: Name of the setting in the appsettings that holds the to the redis cache connection string (eg `<cacheName>.redis.cache.windows.net:6380,password=...`).
- `Keys`: Keys to read from, space-delimited.
  - Uses [`XREADGROUP`](https://redis.io/commands/xreadgroup/).
  - This field can be resolved using `INameResolver`.
- (optional) `PollingIntervalInMs`: How often to poll Redis in milliseconds.
  - Default: 1000
- (optional) `MessagesPerWorker`: How many messages each functions worker "should" process. Used to determine how many workers the function should scale to.
@@ -117,37 +142,32 @@ Inputs:
- (optional) `DeleteAfterProcess`: If the listener will delete the stream entries after the function runs.
  - Default: false

#### Avaiable Output Types
- `RedisStreamEntry`: This class wraps [`StreamEntry` from StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/blob/main/src/StackExchange.Redis/APITypes/StreamEntry.cs).
  - `string Key`: The stream key that the function was triggered on.
  - `string Id`: The ID assigned to the entry.
  - `KeyValuePair<string, string>[] Values`: The values contained within the entry.

#### Sample
The following sample polls the key "streamTest" at a Redis instance defined in local.settings.json at the key "redisConnectionStringSetting"
```c#
[FunctionName(nameof(StreamsTrigger))]
public static void StreamsTrigger(
    [RedisStreamsTrigger("redisConnectionStringSetting", "streamTest")] RedisStreamEntry entry,
    ILogger logger)
{
    logger.LogInformation(JsonSerializer.Serialize(entry));
}
```

### Return Value
All triggers return a [`RedisMessageModel`](./src/Models/RedisMessageModel.cs) object that has two fields:
```c#
namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
  public class RedisMessageModel
  {
    public string Trigger { get; set; }
    public string Message { get; set; }
  }
}
```
- `Trigger`: The pubsub channel, list key, or stream key that the function is listening to.
- `Message`: The pubsub message, list element, or stream element.

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
