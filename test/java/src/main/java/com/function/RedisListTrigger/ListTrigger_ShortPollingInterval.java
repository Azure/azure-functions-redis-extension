package com.function.RedisListTrigger;

import com.function.Common;
import com.microsoft.azure.functions.*;
import com.microsoft.azure.functions.annotation.*;
import com.microsoft.azure.functions.redis.annotation.*;

public class ListTrigger_ShortPollingInterval {
    @FunctionName("ListTrigger_ShortPollingInterval")
    public void run(
            @RedisListTrigger(
                name = "req",
                connectionStringSetting = Common.ConnectionString,
                key = "ListTrigger_ShortPollingInterval",
                pollingIntervalInMs = Common.ShortPollingInterval)
                String message,
            final ExecutionContext context) {
            context.getLogger().info(message);
    }
}
