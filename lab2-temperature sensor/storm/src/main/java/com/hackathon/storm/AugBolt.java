package com.hackathon.storm;

import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseBasicBolt;
import backtype.storm.tuple.Fields;
import backtype.storm.tuple.Tuple;
import backtype.storm.tuple.Values;
import com.google.gson.Gson;
import org.joda.time.DateTime;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/**
 * Created by david on 03/12/14.
 */
public class AugBolt extends BaseBasicBolt {
    @Override
    public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector) {

        String timestamp = tuple.getStringByField("timestamp");
        String deviceid = tuple.getStringByField("deviceid");
        int reading = tuple.getIntegerByField("reading");

        DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
        String servertimestamp = targetFormat.format(new Date());

        basicOutputCollector.emit("energystream", new Values(timestamp, deviceid, reading, servertimestamp));
    }

    @Override
    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {
        outputFieldsDeclarer.declareStream("energystream", new Fields("timestamp", "deviceid", "reading", "servertimestamp"));
    }
}
