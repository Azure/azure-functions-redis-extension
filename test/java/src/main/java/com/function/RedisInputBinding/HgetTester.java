package com.function.RedisInputBinding;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class HgetTester {
    @FunctionName("HgetTester")
    public void run(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = Common.ConnectionString,
                channel = "__keyevent@0__:hset")
                Common.ChannelMessage message,
            @RedisInput(
                name = "value",
                connectionStringSetting = Common.ConnectionString,
                command = "HGET {Message} field")
                String value,
            final ExecutionContext context) {
            context.getLogger().info("Value of field 'field' in hash at key '" + message.Message + "' is currently '" + value + "'");
            
    }
}
