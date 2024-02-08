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
 * Java annotation used to bind a parameter to a Redis pub/sub message.
 * </p>
 *
 * <p>
 * Example function that uses a RedisPubSubTrigger binding:
 * </p>
 *
 * <pre>
 * &#64;FunctionName("RedisPubSubExample")
 * public void run(
 *         &#64;RedisPubSubTrigger(connection = "ConnectionString", channel = "redischannel") String message,
 *         final ExecutionContext context) {
 *     context.getLogger().info("Java Redis PubSub trigger function processed a message: " + message);
 * }
 * </pre>
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "in", name = "", type = "redisPubSubTrigger")
public @interface RedisPubSubTrigger {
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
     * Redis pubsub channel. Supports channel patterns.
     * @return Redis pubsub channel.
     */
    String channel();
}
