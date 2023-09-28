package com.function.RedisListTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class FastPollingListTrigger {
    @FunctionName("FastPollingListTrigger")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = "redisLocalhost",
                key = "listTest",
                pollingIntervalInMs = 100,
                maxBatchSize = 1)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
