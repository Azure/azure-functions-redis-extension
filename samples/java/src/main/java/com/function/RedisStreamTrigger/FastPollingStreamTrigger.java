package com.function.RedisStreamTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class FastPollingStreamTrigger {
    @FunctionName("FastPollingStreamTrigger")
    public void run(
            @RedisStreamTrigger(
                name = "entry",
                connectionStringSetting = "redisLocalhost",
                key = "streamTest",
                pollingIntervalInMs = 100,
                maxBatchSize = 1)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
