/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */
package com.microsoft.azure.functions.redis.annotation;

/**
 * The direction to pop elements from the list.
 */
public enum ListDirection
{
    /**
     * Uses <a href="https://redis.io/commands/lmpop/">LPOP</a> or <a href="https://redis.io/commands/lmpop/">LMPOP RIGHT</a>
     */
    LEFT,

    /**
     * Uses <a href="https://redis.io/commands/rpop/">RPOP</a> or <a href="https://redis.io/commands/lmpop/">LMPOP RIGHT</a>
     */
    RIGHT
}
