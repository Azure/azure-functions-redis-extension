/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.microsoft.azure.functions.redis.annotation;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;
import com.microsoft.azure.functions.annotation.CustomBinding;

/**
 * <p>
 * Java annotation for output binding that excutes a command on the redis instances.
 * </p>
 *
 * <p>
 * Example function that uses a RedisOutput binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisOutputExample")
 * public void run(
 *         &#64;RedisPubSubTrigger(connection = "ConnectionString", channel = "__keyevent@0__:set") String key,
 *         &#64;RedisOutput(connection = "ConnectionString", command = "DEL") OutputBinding<String> command,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Deleting recently SET '" + key + "'");
 *     command.setValue(key);
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target({ ElementType.PARAMETER, ElementType.METHOD })
@CustomBinding(direction = "out", name = "", type = "redis")
public @interface RedisOutput {
    /**
     * The variable name used in function.json.
     * @return The variable name used in function.json.
     */
    String name();

    /**
     * App setting name that contains Redis connection information.
     */
    String connection();

    /**
     * The command to be executed on the cache without any arguments.
     * @return The command to be executed on the cache without any arguments.
     */
    String command();
}
