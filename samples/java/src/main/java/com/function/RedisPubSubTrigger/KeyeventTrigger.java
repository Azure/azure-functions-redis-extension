package com.function.RedisPubSubTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class KeyeventTrigger {
    @FunctionName("KeyeventTrigger")
    public void run(
            @RedisPubSubTrigger(
                name = "req",
                connectionStringSetting = "redisConnectionString",
                channel = "__keyevent@0__:del")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
