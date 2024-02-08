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
 * Java annotation used to bind a parameter to a Redis list entry.
 * </p>
 *
 * <p>
 * Example function that uses a RedisListTrigger binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisListExample")
 * public void run(
 *         &#64;RedisListTrigger(connection = "ConnectionString", key = "listkey") String entry,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Java Redis List trigger function processed a list entry: " + entry);
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "in", name = "", type = "redisListTrigger")
public @interface RedisListTrigger {
    /**
     * The variable name used in function.json.
     * @return The variable name used in function.json.
     */
    String name();

    /**
     * <p>Defines how Functions runtime should treat the parameter value. Possible values are:</p>
     * <ul>
     *     <li>"": get the value as a string, and try to deserialize to actual parameter type like POJO</li>
     *     <li>string: always get the value as a string</li>
     *     <li>binary: get the value as a binary data, and try to deserialize to actual parameter type byte[]</li>
     * </ul>
     * @return The dataType which will be used by the Functions runtime.
     */
    String dataType() default "";

    /**
     * Setting name for Redis connection string.
    * @return Setting name for Redis connection string.
    */
    String connection();

    /**
     * Key to read from.
     * @return Key to read from.
     */
    String key();

    /**
     * How often to poll Redis in milliseconds.
     * @return How often to poll Redis in milliseconds.
     */
    int pollingIntervalInMs() default 1000;

    /**
     * Number of entries to pull from Redis at one time.
     * @return Number of entries to pull from Redis at one time.
     */
    int maxBatchSize() default 16;

    /**
     * The direction to pop elements from the list..
     * @return The direction to pop elements from the list.
     */
    ListDirection listDirection() default ListDirection.LEFT;
}
