package com.hackathon.storm;

import backtype.storm.Testing;
import backtype.storm.task.TopologyContext;
import backtype.storm.testing.MkTupleParam;
import backtype.storm.topology.BasicOutputCollector;
import backtype.storm.topology.IBasicBolt;
import backtype.storm.tuple.Tuple;
import backtype.storm.tuple.Values;
import com.google.gson.Gson;
import com.hackathon.storm.Utils.MockBasicOutputCollector;
import com.hackathon.storm.Utils.MockOutputCollector;
import junit.framework.Test;
import junit.framework.TestCase;
import junit.framework.TestSuite;
import org.joda.time.DateTime;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

import static org.mockito.Mockito.mock;

/**
 * Unit test for simple App.
 */
public class AppTest 
    extends TestCase
{
    /**
     * Create the test case
     *
     * @param testName name of the test case
     */
    public AppTest( String testName )
    {
        super( testName );
    }

    /**
     * @return the suite of tests being tested
     */
    public static Test suite()
    {
        return new TestSuite( AppTest.class );
    }

    /**
     * Rigourous Test :-)
     */
    public void testApp()
    {

       /*//* MockOutputCollector out = new MockOutputCollector();
        MockBasicOutputCollector mockoc = new MockBasicOutputCollector(out);
       BasicOutputCollector collector = new BasicOutputCollector(mockoc);

        TopologyContext context = mock(TopologyContext.class);
        Map conf = mock(Map.class);

        IBasicBolt bolt = new SqlStorageBolt();
        Map<String, String> field1 = new HashMap<String, String>();


        MkTupleParam param = new MkTupleParam();
        param.setStream("test-stream");
        param.setComponent("test-component");
        param.setFields("timestamp", "deviceid", "reading");
        Tuple tuple = Testing.testTuple(new Values("2014-12-04T12:10:46.394Z", "Device01", 5), param);


        bolt.prepare(conf, context);
        bolt.execute(tuple,(BasicOutputCollector)collector);*//**//*
*/
    /*    Gson gson = new Gson();
        Map<String, Object> obj = new HashMap<String, Object>();
        obj.put("deviceId", "a");
        obj.put("timestamp", 5);
        obj.put("reading ", "c");
        obj.put("servertimestamp", "sdf");
        System.out.println(gson.toJson(obj));*/


       /* DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
        String servertimestamp = targetFormat.format(new Date());
        System.out.println(servertimestamp);*/

    }
}
