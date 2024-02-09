package com.function;

import java.util.Map;

public static class Common {

    public static String ConnectionString = "redisConnectionString";
    public static String ManagedIdentity = "redisManagedIdentity";
    public static int ShortPollingInterval = 100;
    public static int LongPollingInterval = 10000;

    public static class StreamEntry {
        public String Id;
        public Map<String, String> Values;

        public StreamEntry(String id, Map<String, String> values)
        {
            Id = id;
            Values = values;
        }
    }

    public static class ChannelMessage {
        public String SubscriptionChannel;
        public String Channel;
        public String Message;

        public ChannelMessage(String subscriptionChannel, String channel, String message)
        {
            SubscriptionChannel = subscriptionChannel;
            Channel = channel;
            Message = message;
        }
    }
}
