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
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.*;

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
    public void testApp() throws ParseException {

        MockOutputCollector out = new MockOutputCollector();
        MockBasicOutputCollector mockoc = new MockBasicOutputCollector(out);
       BasicOutputCollector collector = new BasicOutputCollector(mockoc);

        TopologyContext context = mock(TopologyContext.class);
        Map conf = mock(Map.class);

        IBasicBolt bolt = new RedisStorageBolt();
        Map<String, String> field1 = new HashMap<String, String>();


        MkTupleParam param = new MkTupleParam();
        param.setStream("test-stream");
        param.setComponent("test-component");

        param.setFields("timestamp", "deviceid", "startReading", "endReading", "energyUsage", "servertimestamp");

        Tuple tuple = null;
        int endReading = 5000;
        int startReading = 5000;


        Calendar cal = new GregorianCalendar();
        cal.set(Calendar.HOUR_OF_DAY, 0);
        cal.set(Calendar.MINUTE, 0);
        cal.set(Calendar.SECOND, 0);

        for(int i=0; i < 24* 60*60; i++)
        {
            int plusOrMinus = Math.random() < 0.5 ? -1 : 1;

            Random r = new Random();
            int amount = r.nextInt(100);
            startReading = endReading;
            endReading += 1000+(plusOrMinus * amount);
            int energyUsage = endReading - startReading;

            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
            String time = sdf.format(cal.getTime());

            tuple = Testing.testTuple(new Values(time, "Device01", startReading, endReading, energyUsage, "2014-12-04T12:10:46.394Z"), param);
            bolt.prepare(conf, context);
            bolt.execute(tuple,(BasicOutputCollector)collector);

            cal.add(Calendar.SECOND, 1);
        }









        /*Gson gson = new Gson();
        Map<String, Object> obj = new HashMap<String, Object>();
        obj.put("deviceId", "a");
        obj.put("timestamp", 5);
        obj.put("reading ", "c");
        obj.put("servertimestamp", "sdf");
        System.out.println(gson.toJson(obj));*/


       /* DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
        String servertimestamp = targetFormat.format(new Date());
        System.out.println(servertimestamp);
        */
/*        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd'T'hh:mm:ss.SSS'Z'");
        Date dateStr = formatter.parse("2014-12-04T12:10:46.394Z");*/

 /*       System.out.format("%02d",12);*/
    }
}
