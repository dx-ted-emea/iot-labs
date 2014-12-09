    package com.hackathon.storm;

    import backtype.storm.topology.BasicOutputCollector;
    import backtype.storm.topology.OutputFieldsDeclarer;
    import backtype.storm.topology.base.BaseBasicBolt;
    import backtype.storm.tuple.Tuple;
    import com.google.gson.Gson;
    import redis.clients.jedis.Jedis;
    import redis.clients.jedis.JedisPool;

    import java.util.HashMap;
    import java.util.Map;
    import java.util.Properties;

    public class RedisStorageBolt extends BaseBasicBolt {
        @Override
        public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector) {

            Gson gson = new Gson();
            try {
                //Get the deviceid and temperature by field name
                String timestamp = tuple.getStringByField("timestamp");
                String deviceid = tuple.getStringByField("deviceid");
                int reading = tuple.getIntegerByField("reading");
                String servertimestamp = tuple.getStringByField("servertimestamp");

                Map<String, Object> obj = new HashMap<String, Object>();
                obj.put("deviceId", deviceid);
                obj.put("timestamp", timestamp);
                obj.put("reading ", reading);
                obj.put("servertimestamp", servertimestamp);

                Properties properties = new Properties();
                properties.load(this.getClass().getClassLoader().getResourceAsStream("Config.properties"));

                String hostname = properties.getProperty("redis.host");
                int port = Integer.parseInt(properties.getProperty("redis.port"));
                String password = properties.getProperty("redis.password");
                int ttl = Integer.parseInt(properties.getProperty("redis.ttl"));

                JedisPool pool = new JedisPool(hostname, port);
                Jedis connection = pool.getResource();
                connection.auth(password);
                connection.set(timestamp, (String) gson.toJson(obj));
                connection.expire(timestamp, ttl);
            } catch (Exception e) {
                // LOG.error("Bolt execute error: {}", e);
                basicOutputCollector.reportError(e);
            }

        }

        @Override
        public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {

        }
    }
