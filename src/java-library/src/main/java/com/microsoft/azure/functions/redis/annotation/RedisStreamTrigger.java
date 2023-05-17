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
 *         &#64;RedisStreamTrigger(connectionStringSetting = "ConnectionString", key = "streamkey") String entry,
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
    String connectionStringSetting();

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
     * How many messages each functions worker should process.
     * Used to determine how many workers the function should scale to.
     * For example, if the messagesPerWorker is 10,
     * and there are 1500 elements remaining in the list,
     * the functions host will attempt to scale up to 150 instances.
     * @return How many messages each functions worker should process.
     */
    int messagesPerWorker() default 100;

    /**
     * Number of elements to pull from Redis at one time.
     * @return Number of elements to pull from Redis at one time.
     */
    int count() default 10;

    /**
     * If the listener will delete the stream entries after the function runs.
     * @return If the listener will delete the stream entries after the function runs.
     */
    boolean deleteAfterProcess() default false;
}
