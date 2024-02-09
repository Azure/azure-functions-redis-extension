package com.function.RedisListTrigger;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class ListTrigger_ManagedIdentity {
    @FunctionName("ListTrigger_ManagedIdentity")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = Common.ManagedIdentity,
                key = "ListTrigger_ManagedIdentity")
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
