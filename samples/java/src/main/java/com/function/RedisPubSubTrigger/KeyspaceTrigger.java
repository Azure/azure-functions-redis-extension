package com.function.RedisPubSubTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class KeyspaceTrigger {
    @FunctionName("KeyspaceTrigger")
    public void run(
            @RedisPubSubTrigger(
                name = "req",
                connection = "redisConnectionString",
                channel = "__keyspace@0__:keyspaceTest")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
