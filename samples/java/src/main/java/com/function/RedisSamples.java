package com.function;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class RedisSamples {
    @FunctionName("PubSubTrigger")
    public void PubSubTrigger(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = "redisLocalhost",
                channel = "pubsubTest")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }

    @FunctionName("PubSubTriggerResolvedChannel")
    public void PubSubTriggerResolvedChannel(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = "redisLocalhost",
                channel = "%pubsubChannel%")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }

    @FunctionName("KeyspaceTrigger")
    public void KeyspaceTrigger(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = "redisLocalhost",
                channel = "__keyspace@0__:keyspaceTest")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }

    @FunctionName("KeyeventTrigger")
    public void KeyeventTrigger(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = "redisLocalhost",
                channel = "__keyevent@0__:del")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }

    @FunctionName("ListTrigger")
    public void ListTrigger(
            @RedisListTrigger(
                name = "entry",
                connectionStringSetting = "redisLocalhost",
                key = "listTest",
                pollingIntervalInMs = 100,
                count = 1,
                listPopFromBeginning = false)
                String entry,
            final ExecutionContext context) {
            context.getLogger().info(entry);
    }

    @FunctionName("StreamTrigger")
    public void StreamTrigger(
            @RedisStreamTrigger(
                name = "entry",
                connectionStringSetting = "redisLocalhost",
                key = "streamTest",
                pollingIntervalInMs = 100,
                count = 1,
                deleteAfterProcess = true)
                String entry,
            final ExecutionContext context) {
            context.getLogger().info(entry);
    }
}
