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

/**
 * <p>
 * Java annotation used to bind a parameter to Redis list entry.
 * </p>
 *
 * <p>
 * Example function that uses a RedisListTrigger binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisListExample")
 * public void run(
 *         &#64;RedisListTrigger(connectionStringSetting = "ConnectionString", keys = "listkey") String entry,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Java Redis List trigger function processed a list entry: " + entry);
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
public @interface RedisListTrigger {
    /**
     * The variable name used in function.json.
     * @return The variable name used in function.json.
     */
    String name();

    /**
     * Setting name for Redis connection string.
     * @return Setting name for Redis connection string.
     */
    String connectionStringSetting() default "";

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
     * Keys to read from, space-delimited.
     * Multiple keys only supported on Redis 7.0+ using <a href=https://redis.io/commands/lmpop/><code>LMPOP</code></a>.
     * Listens to only the first key given in the argument using <a href=https://redis.io/commands/lpop/><code>LPOP</code></a>/<a href=https://redis.io/commands/rpop/><code>RPOP</code></a> on Redis versions less than 7.0.
     * @return Keys to read from, space-delimited.
     */
    String keys();

    /**
     * How often to poll Redis in milliseconds.
     * @return How often to poll Redis in milliseconds.
     */
    int pollingIntervalInMs();

    /**
     * How many messages each functions worker should process. Used to determine how many workers the function should scale to.
     * @return How many messages each functions worker should process.
     */
    int messagesPerWorker();

    /**
     * Number of elements to pull from Redis at one time.
     * Only supported on Redis 6.2+ using the <code>COUNT</code> argument in <a href=https://redis.io/commands/lpop/><code>LPOP</code></a>/<a href=https://redis.io/commands/rpop/><code>RPOP</code></a>.
     * Defaults to 1 on Redis versions less than 6.2.
     * @return Number of elements to pull from Redis at one time.
     */
    int batchSize();

    /**
     * Determines whether to pop elements from the beginning using <a href=https://redis.io/commands/lpop/><code>LPOP</code></a> or to pop elements from the end using <a href=https://redis.io/commands/rpop/><code>RPOP</code></a>.
     * @return Whether to pop elements from the beginning or end of the list.
     */
    boolean listPopFromBeginning();
}
