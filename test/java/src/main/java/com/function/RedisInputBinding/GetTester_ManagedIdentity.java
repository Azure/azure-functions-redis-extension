package com.function.RedisInputBinding;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class GetTester_ManagedIdentity {
    @FunctionName("GetTester_ManagedIdentity")
    public void run(
            @RedisPubSubTrigger(
                name = "message",
                connectionStringSetting = Common.ConnectionString,
                channel = "__keyevent@0__:set")
                Common.ChannelMessage message,
            @RedisInput(
                name = "value",
                connectionStringSetting = Common.ConnectionString,
                command = "GET {Message}")
                String value,
            final ExecutionContext context) {
            context.getLogger().info("Key '" + message.Message + "' was set to value '" + value + "'");
    }
}
