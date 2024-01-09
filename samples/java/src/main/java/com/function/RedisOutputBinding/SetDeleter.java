package com.function.RedisOutputBinding;

import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class SetDeleter {
    @FunctionName("SetDeleter")
    @RedisOutput(
                name = "value",
                connectionStringSetting = "redisConnectionString",
                command = "DEL")
    public String run(
            @RedisPubSubTrigger(
                name = "key",
                connectionStringSetting = "redisConnectionString",
                channel = "__keyevent@0__:set")
                String key,
            final ExecutionContext context) {
        context.getLogger().info("Deleting recently SET key '" + key + "'");
        return key;
    }
}
