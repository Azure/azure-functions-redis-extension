package com.function.RedisStreamTrigger;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SimpleStreamTrigger {
    @FunctionName("SimpleStreamTrigger")
    public void run(
            @RedisStreamTrigger(
                name = "req",
                connectionStringSetting = "redisConnectionString",
                key = "streamTest",
                pollingIntervalInMs = 1000,
                maxBatchSize = 1)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
