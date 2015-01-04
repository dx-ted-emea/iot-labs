package com.hackathon.storm;

import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseBasicBolt;
import backtype.storm.tuple.Tuple;
import com.google.gson.Gson;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import redis.clients.jedis.exceptions.JedisConnectionException;

import java.text.SimpleDateFormat;
import java.util.*;

public class RedisStorageBolt extends BaseBasicBolt {
    @Override
    public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector) {

        Gson gson = new Gson();
        try {
            //Get the values by field
            String timestamp = tuple.getStringByField("timestamp");
            String deviceid = tuple.getStringByField("deviceid");
            int startReading = tuple.getIntegerByField("startReading");
            int endReading = tuple.getIntegerByField("endReading");
            int energyUsage = tuple.getIntegerByField("energyUsage");
            String servertimestamp = tuple.getStringByField("servertimestamp");

            //create an object we can json stringify
            Map<String, Object> obj = new HashMap<String, Object>();
            obj.put("deviceId", deviceid);
            obj.put("timestamp", timestamp);
            obj.put("startReading", startReading);
            obj.put("endReading", endReading);
            obj.put("energyUsage", energyUsage);
            obj.put("servertimestamp", servertimestamp);

            Properties properties = new Properties();
            properties.load(this.getClass().getClassLoader().getResourceAsStream("Config.properties"));

            //connect to redis
            String hostname = properties.getProperty("redis.host");
            int port = Integer.parseInt(properties.getProperty("redis.port"));
            String password = properties.getProperty("redis.password");
            int ttl = Integer.parseInt(properties.getProperty("redis.ttl"));

            SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
            Date dateStr = formatter.parse(timestamp);
            Calendar calendar = new GregorianCalendar();
            calendar.setTime(dateStr);

            String key = String.format("%s%02d%02d%s", calendar.get(Calendar.YEAR), calendar.get(Calendar.MONTH) + 1, calendar.get(Calendar.DAY_OF_MONTH), deviceid);

            JedisPool pool =  RedisConnection.getPool(hostname, port);
            Jedis connection = null;

            try {
                connection = pool.getResource();
                connection.auth(password);
                connection.rpush(key, (String) gson.toJson(obj));
                connection.expire(key, ttl);
            } catch (JedisConnectionException jex) {
                pool.returnBrokenResource(connection);
            } finally {
                pool.returnResource(connection);
            }

            pool.returnResource(connection);
        } catch (Exception e) {
            // LOG.error("Bolt execute error: {}", e);
            basicOutputCollector.reportError(e);
        }

    }

    @Override
    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {

    }
}
