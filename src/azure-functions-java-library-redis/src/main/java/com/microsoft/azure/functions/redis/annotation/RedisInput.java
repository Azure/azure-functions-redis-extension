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
 * Java annotation for input binding that excutes a command on the redis instances.
 * </p>
 *
 * <p>
 * Example function that uses a RedisInput binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisInputExample")
 * public void run(
 *         &#64;RedisPubSubTrigger(connection = "ConnectionString", channel = "__keyevent@0__:set") String key,
 *         &#64;RedisInput(connection = "ConnectionString", command = "GET {Message}") String value,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Key '" + key + '" was set to "' + value + '"');
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "in", name = "", type = "redis")
public @interface RedisInput {
    /**
     * The variable name used in function.json.
     * @return The variable name used in function.json.
     */
    String name();

    /**
     * Setting name for Redis connection string.
     * @return Setting name for Redis connection string.
     */
    String connection();

    /**
     * The command to be executed on the cache.
     * @return The command to be executed on the cache.
     */
    String command();
}
