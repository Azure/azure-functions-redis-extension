package com.function.RedisListTrigger;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class ListTrigger_RightDirection {
    @FunctionName("ListTrigger_RightDirection")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = Common.ConnectionString,
                key = "ListTrigger_RightDirection",
                listDirection = ListDirection.RIGHT)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
