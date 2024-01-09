package com.function;

import java.util.Map;

public class RedisStreamEntry {
    public String Id;
    public Map<String, String> Values;

    public RedisStreamEntry(String id, Map<String, String> values)
    {
        Id = id;
        Values = values;
    }
}
