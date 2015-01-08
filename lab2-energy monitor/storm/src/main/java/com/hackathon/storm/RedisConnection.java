package com.hackathon.storm;

import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import redis.clients.jedis.Protocol;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.io.IOException;


public class RedisConnection  {

    private static JedisPool pool;

    public static JedisPool getPool(String ipAddress, int port)
    {
        if(pool == null)
        {
            JedisPoolConfig conf = new JedisPoolConfig();
            conf.setMaxTotal(128);
            pool = new JedisPool(conf, ipAddress, port,  6);
        }

        return pool;
    }
}
