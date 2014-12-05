package com.hackathon.storm;

import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.OutputFieldsDeclarer;
import backtype.storm.topology.base.BaseBasicBolt;
import backtype.storm.tuple.Tuple;

import java.io.IOException;
import java.sql.*;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Locale;
import java.util.Properties;
import java.util.Date;

public class SqlStorageBolt extends BaseBasicBolt {
    @Override
    public void execute(Tuple tuple, BasicOutputCollector basicOutputCollector){
        Connection connection = null;
        Statement statement = null;
        try {
            Properties properties = new Properties();
            properties.load(this.getClass().getClassLoader().getResourceAsStream("Config.properties"));
            String connectionString = properties.getProperty("sql.connection");

            String timestamp = tuple.getStringByField("timestamp");
            String deviceid = tuple.getStringByField("deviceid");
            int reading = tuple.getIntegerByField("reading");
            String servertimestamp = tuple.getStringByField("servertimestamp");

            DateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss.SSS'Z'", Locale.ENGLISH);
            Date date =  format.parse(timestamp);

            DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
            String targetDate = targetFormat.format(date);

            Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
            connection = DriverManager.getConnection(connectionString);

            String query = "INSERT into readings VALUES ('" + targetDate +"', '" + deviceid +"', " + reading + ", '" + servertimestamp + "')";
            statement = connection.createStatement();
            statement.executeQuery(query);

        }catch (ClassNotFoundException | SQLException | ParseException | IOException ex ) {
            ex.printStackTrace();
            System.out.println(ex.getMessage());
        }
        finally
        {
            try
            {
                // Close resources.
                if (null != connection) connection.close();
                if (null != statement) statement.close();
            }
            catch (SQLException sqlException) {
                // No additional action if close() statements fail.
            }
        }
    }

    @Override
    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {

    }
}
