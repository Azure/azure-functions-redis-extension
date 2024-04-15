package com.function.RedisPubSubTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SimplePubSubTrigger {
    @FunctionName("SimplePubSubTrigger")
    public void run(
            @RedisPubSubTrigger(
                name = "req",
                connection = "redisConnectionString",
                channel = "pubsubTest",
                pattern = false)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
