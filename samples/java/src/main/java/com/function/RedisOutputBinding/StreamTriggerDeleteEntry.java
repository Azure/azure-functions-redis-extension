package com.function.RedisOutputBinding;

import com.function.RedisStreamEntry;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class StreamTriggerDeleteEntry {
    @FunctionName("StreamTriggerDeleteEntry")
    public void run(
            @RedisStreamTrigger(
                name = "entry",
                connection = "redisConnectionString",
                key = "streamTest2")
                RedisStreamEntry entry,
            @RedisOutput(
                name = "value",
                connection = "redisConnectionString",
                command = "DEL")
                OutputBinding<String> value,
            final ExecutionContext context) {
                
                context.getLogger().info("Stream entry from key 'streamTest2' with Id '" + entry.Id + "'");
                value.setValue("streamTest2 " + entry.Id);
    }
}
