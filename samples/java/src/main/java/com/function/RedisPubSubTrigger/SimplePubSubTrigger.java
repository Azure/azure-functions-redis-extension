package com.function.RedisPubSubTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SimplePubSubTrigger {
    @FunctionName("SimplePubSubTrigger")
    public void run(
            @RedisPubSubTrigger(
                name = "req",
                connectionStringSetting = "redisConnectionString",
                channel = "pubsubTest")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
