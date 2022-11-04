
# Redis Extension for Azure Functions

## Introduction

This repository contains the Redis trigger bindings to use in your [Azure Functions](https://learn.microsoft.com/azure/azure-functions/functions-get-started) and [WebJobs](https://learn.microsoft.com/azure/app-service/webjobs-sdk-how-to). The trigger binding enables invoking a function when message is received on a [Redis PubSub channel](https://redis.io/docs/manual/pubsub/) or on a [KeySpace or KeyEvent Notification](https://redis.io/docs/manual/keyspace-notifications/).

The Azure Functions Redis Extension allows you to trigger functions based on Redis pub/sub messages, Redis Keyspace notifications, and Redis Keyevent notifications from your Azure Redis Cache all via the [Azure functions interface](https://docs.microsoft.com/azure/azure-functions/functions-create-your-first-function-visual-studio).

To get started with developing with this extension, make sure you first [set up an Azure Cache for Redis instance](https://docs.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis). Then you can go ahead and begin developing your functions.

## Usage
### RedisTrigger Parameters
Input: RedisTrigger takes in three arguments:
- `ConnectionString`: connection string to the redis cache (eg `<cacheName>.redis.cache.windows.net:6380,password=...`)
- `TriggerType`: This is an `RedisTriggerType` enum that can be one of three values: `PubSub`, `KeySpace`, or `KeyEvent`
- `Trigger`:
  - If `TriggerType` is `PubSub`, this is the name of the pubsub channel on which to trigger
  - If `TriggerType` is `KeySpace`, this is the key on which to trigger
  - If `TriggerType` is `KeyEvent`, this is the keyevent on which to trigger

The RedisTrigger returns a `RedisMessageModel` object that has three fields:
```c#
namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
  public class RedisMessageModel
  {
    public RedisTriggerType TriggerType { get; set; }
    public string Trigger { get; set; }
    public string Message { get; set; }
  }
}
```
- `TriggerType`: The `RedisTriggerType` that the function uses
- `Trigger`: The pubsub channel, key, or keyevent on which the function was triggered
- `Message`:
  - If `TriggerType` is `PubSub`, this will be the message received by the pubsub channel
  - If `TriggerType` is `KeySpace`, this will be the keyevent that occurred on the key
  - If `TriggerType` is `KeyEvent`, this will be the key on which the keyevent occurred

> **Note**
>
> If the input `Trigger` value is a glob pattern, the output `Trigger` value will be the exact pubsub channel, key, or keyevent, not the input `Trigger` value.
>
> For example, if the inputs to the function are `TriggerType=PubSub, Trigger="test*"`, and a message is published to the channel `"test2"`, the `Trigger` in the `RedisCacheMessageModel` will be `"test2"`.

### Example Function
The following example shows a [C# function](https://learn.microsoft.com/azure/azure-functions/functions-dotnet-class-library) that gets invoked (by virtue of the trigger binding) when a message is published to a Redis channel queue named `channel`.

```c#
[FunctionName("PubSubTrigger")]
public static void PubSubTrigger(
  [RedisTrigger(ConnectionString = "<cacheName>.redis.cache.windows.net:6380", TriggerType = RedisTriggerType.PubSub, Trigger = "channel")]
  RedisMessageModel result, ILogger logger)
{
  logger.LogInformation(JsonSerializer.Serialize(result));
}
```

## Getting Started
First, [create a Redis cache](https://learn.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis).

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
