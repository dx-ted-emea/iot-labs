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

        IBasicBolt bolt = new ParseBolt();
        Map<String, String> field1 = new HashMap<String, String>();

        MkTupleParam param = new MkTupleParam();
        param.setStream("test-stream");
        param.setComponent("test-component");

        param.setFields("string");

        Tuple tuple = null;

        Calendar cal = new GregorianCalendar();
        cal.set(Calendar.HOUR_OF_DAY, 0);
        cal.set(Calendar.MINUTE, 0);
        cal.set(Calendar.SECOND, 0);

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        String time = sdf.format(cal.getTime());

        String str = String.format("{ \"deviceid\" : \"device0\", \"timestamp\" : \"%s\", \"startReading\" : %d, \"endReading\" : %d, \"energyUsage\" : %d }", time.toString(), 5000, 5080, 80);

        tuple = Testing.testTuple(new Values(str), param);
        bolt.prepare(conf, context);
        bolt.execute(tuple,(BasicOutputCollector)collector);

        List<Values> result = mockoc.getEmittedValues();

        assert(result.size() == 1 && result.get(0).size() == 5);
    }
}
