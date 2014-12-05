package com.hackathon.storm;
import backtype.storm.topology.base.BaseBasicBolt;
import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.tuple.Tuple;
import backtype.storm.tuple.Fields;
import backtype.storm.tuple.Values;

import com.google.gson.Gson;


public class ParseBolt extends BaseBasicBolt  {
    @Override
    public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector) {
        Gson gson = new Gson();
        //Should only be one tuple, which is the JSON message from the spout
        String value = tuple.getString(0);

        //Deal with cases where we get multiple
        //EventHub messages in one tuple
        String[] arr = value.split("}");
        for (String ehm : arr)
        {

            //Convert it from JSON to an object
            Message msg = new Gson().fromJson(ehm.concat("}"),Message.class);

            //Pull out the values and emit as a stream
            String timestamp = msg.timestamp;
            String deviceid = msg.deviceId;
            int reading = msg.reading;

            basicOutputCollector.emit("energystream", new Values(timestamp, deviceid, reading));
        }
    }

    @Override
    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {
        outputFieldsDeclarer.declareStream("energystream", new Fields("timestamp", "deviceid", "reading"));
    }
}
