package com.function.RedisListTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SimpleListTrigger {
    @FunctionName("SimpleListTrigger")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = "redisConnectionString",
                key = "listTest",
                pollingIntervalInMs = 1000,
                maxBatchSize = 1)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
