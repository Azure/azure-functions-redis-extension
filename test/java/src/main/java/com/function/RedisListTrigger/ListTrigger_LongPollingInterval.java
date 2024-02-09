package com.function.RedisListTrigger;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class ListTrigger_LongPollingInterval {
    @FunctionName("ListTrigger_LongPollingInterval")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = Common.ConnectionString,
                key = "ListTrigger_LongPollingInterval",
                pollingIntervalInMs = Common.LongPollingInterval)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
