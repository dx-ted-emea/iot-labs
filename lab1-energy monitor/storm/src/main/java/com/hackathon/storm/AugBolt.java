package com.hackathon.storm;

import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseBasicBolt;
import backtype.storm.tuple.Fields;
import backtype.storm.tuple.Tuple;
import backtype.storm.tuple.Values;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class AugBolt extends BaseBasicBolt {
    @Override
    public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector) {

        String timestamp = tuple.getStringByField("timestamp");
        String deviceid = tuple.getStringByField("deviceid");
        int startReading = tuple.getIntegerByField("startReading");
        int endReading = tuple.getIntegerByField("endReading");
        int energyUsage = tuple.getIntegerByField("energyUsage");

        DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
        String servertimestamp = targetFormat.format(new Date());

        basicOutputCollector.emit("energystream", new Values(timestamp, deviceid, startReading, endReading, energyUsage, servertimestamp));
    }

    @Override
    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {
        outputFieldsDeclarer.declareStream("energystream", new Fields("timestamp", "deviceid", "startReading", "endReading", "energyUsage", "servertimestamp"));
    }
}
