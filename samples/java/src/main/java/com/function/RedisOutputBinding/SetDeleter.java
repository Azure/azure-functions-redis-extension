package com.function.RedisOutputBinding;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SetDeleter {
    @FunctionName("SetDeleter")
    @RedisOutput(
                name = "value",
                connection = "redisConnectionString",
                command = "DEL")
    public String run(
            @RedisPubSubTrigger(
                name = "key",
                connection = "redisConnectionString",
                channel = "__keyevent@0__:set",
                pattern = false)
                String key,
            final ExecutionContext context) {
        context.getLogger().info("Deleting recently SET key '" + key + "'");
        return key;
    }
}
