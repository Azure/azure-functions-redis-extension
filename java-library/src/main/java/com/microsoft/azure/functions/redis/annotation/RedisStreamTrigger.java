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
 * Java annotation used to bind a parameter to Redis stream entry.
 * </p>
 *
 * <p>
 * Example function that uses a RedisStreamTrigger binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisStreamExample")
 * public void run(
 *         &#64;RedisStreamTrigger(connectionStringSetting = "ConnectionString", keys = "streamkey") String entry,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Java Redis Stream trigger function processed a entry: " + entry);
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
public @interface RedisStreamTrigger {
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
     * Uses <a href=https://redis.io/commands/xreadgroup/><code>XREADGROUP</code></a>.
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
     * @return Number of elements to pull from Redis at one time.
     */
    int batchSize();

    /**
     * The name of the consumer group that the function will use.
     * @return The name of the consumer group that the function will use.
     */
    String consumerGroup();

    /**
     * If the listener will delete the stream entries after the function runs.
     * @return If the listener will delete the stream entries after the function runs.
     */
    boolean deleteAfterProcess();
}
