# IoT Smart home Hackathon Lab #

## Lab 2 Environmental Sensor ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Architecture ###

Here an overview of the Lab2; (Arduino->)Tessel->EventHub->BusinessLogic

# Building an Environment Sensor #

##Components##

The Environment Sensor is made of two main pieces of hardware;

1. External environment sensor; a piece of electronics that senses external environmental factors and emits a signal that represents the state. An example is an input device that alters its voltage based on the temperature to which it is exposed. 
2. Sensor control device; an Arduino, Tessel.io, Netduino or similar; a piece of specialist electronics that can connect to many external sensors, gather their signals and perform an action based upon them. In this example, we will focus on using the Tessel.io to achieve the workload required and presenting an approach that is also device agnostic.

Starting with the Sensor control device, we can explore these components.

### Sensor Control Device ###

The Sensor Control Device has the sole function of interfacing with lower level electronics equipment and bridging the logical gap of capability between the devices, allowing the readings from the single function temperature sensor to be wrapped into a network oriented packet and distributed to cloud based services. 

The Sensor Control Device can be constructed in many forms; popular choices include:

1. [Arduino](http://arduino.cc/) controllers, 
2. [Netduino](http://www.netduino.com/) controllers, which are similar to Arduinos but run Microsofts' .net MicroFramework
3. [Raspberry Pi](http://www.raspberrypi.org/)
4. [Tessel](http://tessel.io)

In this lab we will be using a Tessel, which allows the software developer to write their software in Javascript via Nodejs. This convergence of the low level electronics programming with higher level, popular programming languages is common in modern hobbyist hardware development as it allows hardware vendors to target large audiences. 

### External Environment Sensor ###

This lab takes a basic approach to using the Tessel; using the available modules to determine the temperature. If you want to explore the hardware aspect of this lab in more detail, explore the lower level [Lab 2.1, Arduino](Lab2.1-Arduino.md) chaining. 

In order to detect temperature in an environment, there are many options electronically available to you. You can use a thermometer and rig up some computer vision, a thermister (temperature sensitive resister) or bimetallic strips. To get a more accurate sensor output it is now common to use solid-state technology; as heat is applied to a diode the voltage across it increases in a known way. Typically this around 10mV per degree difference in ambient temperature. This approach is known as a ["silicon bandgap" sensor ](http://en.wikipedia.org/wiki/Silicon_bandgap_temperature_sensor)

Not only is this sensor very accurate compared to the other techniques, it is durable to conditions from -40 deg C to + 90 deg C, it never fatigues as it has no moving parts, it doesn't require calibration and costs a couple of dollars. 

This lab will use the [**Si7005**](http://www.silabs.com/Support%20Documents/TechnicalDocs/Si7005.pdf) based [Climate module](https://tessel.io/docs/climate) for the Tessel. This is a simple way of adding capabilities to the Tessel board for temperature and humidity. This approach makes electrical engineering possible without needing to solder or build circuits yourself, but if you agree with the author that the joy of IoT is creativity, the expression of creativity is to make rather than consume, explore the lower level [Lab 2.1, Arduino](Lab2.1-Arduino.md) chaining. 

# Writing a Tessel based Environment Sensor #

The Tessel [documents](https://tessel.io/docs/climate) are a great starting point for this part of the Lab. Let's go over those in order to make sure our Tessel and Climate Module are working well.

## Basic Tessel setup ##

Using the Tessel the user can write Javascript in a nodejs compliant manner, using similar approaches to how one might write any nodejs applications, such as the use of `npm` and `require('modulename')`

Once the required modules are pulled into Javascript the Tessel has the ability to utilise the Climate module in a very simple way. 

Firstly address the climate module in order set up the dependencies:

```javascript
var climatelib = require('climate-si7005');

var climate = climatelib.use(tessel.port['A']);
```

Once you have access to this, you can continually poll on methods such as `readTemperature` in order to gain a periodic value for the climate modules received temperature:

```javascript
climate.on('ready', function () {
  console.log('Connected to si7005');

  // Loop forever
  setImmediate(function loop () {
    climate.readTemperature('f', function (err, temp) {
      climate.readHumidity(function (err, humid) {
        console.log('Degrees:', temp.toFixed(4) + 'F', 'Humidity:', humid.toFixed(4) + '%RH');
        setTimeout(loop, 300);
      });
    });
  });
});

climate.on('error', function(err) {
  console.log('error connecting module', err);
});
setInterval(function(){}, 20000);
```

So far we have replicated the base function of the Tessel with the Climate module. What we should do now is use this data for something interesting and invoke a remote endpoint hosted in the Cloud with a payload based on the reading that we have just started to receive.

## Event Hubs and Azure ##

[question; detail here or refer back to earlier lab1 example?]

[Here detail how to use Event Hub with nodejs/tessel] 

# Building a scalable Cloud service #



**Event Hub**
-

Event Hubs is a highly scalable publish-subscribe ingestor that can intake millions of events per second so that you can process and analyse the massive amounts of data produced by your connected devices and applications. Once collected into Event Hubs, you can transform and store data using any real-time analytics provider or with batching/storage adapters.  The Event Hub will recieve json enconded messages from he Gateway and will be used for both real-time processing via storm and batch based processing via HDInsight/Hadoop.

The following describes the steps to configure an Event Hub to which we can send messages (and consume them)

1. Login to the Azure Portal
2. Navigate to Service Bus and create a new instance
3. Under Event Hubs, create a new instance
4. Once complete navigate to Configure tab
5. Create a new Shared Access Policy and set permissions to Manage, Send, Listen
6. Save the updated configuration
7. Copy the Policy Name and Primary Key after the Save has completed.  These details will be required to connect to the Event Hub.

## Question? SQL? or Redis? ##

**Storm**
- 

Microsoft's Azure-based HD Insight service provides Apache Storm capabilities for streaming-data analysis. Storm makes it easy to reliably process unbounded streams of data, doing for realtime processing what Hadoop did for batch processing.

Five characteristics make Storm ideal for real-time data processing workloads. Storm is:

- **Fast** – benchmarked as processing one million 100 byte messages per second per node
- **Scalable** – with parallel calculations that run across a cluster of machines
- **Fault-tolerant** – when workers die, Storm will automatically restart them. If a node dies, the worker will be restarted on another node.
- **Reliable** – Storm guarantees that each unit of data (tuple) will be processed at least once or exactly once. Messages are only replayed when there are failures.
- **Easy to operate** – standard configurations are suitable for production on day one. Once deployed, Storm is easy to operate.

Primarily storm is composed of 

- **Tuples** – an ordered list of elements. For example, a “4-tuple” might be (7, 1, 3, 7)
- **Streams** – an unbounded sequence of tuples.
- **Spouts** –sources of streams in a computation (e.g. a Twitter API)
- **Bolts** – process input streams and produce output streams. They can: run functions; filter, aggregate, or join data; or talk to databases.
- **Topologies** – the overall calculation, a set of spouts connected to sequences of bolts that can work in parallel

In this example we will construct a simple topology in Java that;

- Reads from the Event Hub
- Parses the JSON message
- Augments the data 
- Outputs to **SQL? Redis?**

**Pre-requisites**

- A Redis cluster or SQL Database setup in Azure
- Java 1.7 JDK installed
- IDE such as Intellij/Eclipse
- Maven installed
- Storm instance created in Azure (**TODO: outline steps)**

To create the example follow these staps

- Enable Remote for the Storm cluster
- Connect and navigate to 

    ` C:\apps\dist\storm-0.9.1.2.1.8.0-2176\examples\eventhubspout`

- Download the file 

	`eventhubs-storm-spout-0.9-jar-with-dependencies.jar`

- Add the jar to the Maven repository using the following command

    `mvn install:install-file -Dfile=target\eventhubs-storm-spout-0.9-jar-with-dependencies.jar -DgroupId=com.microsoft.eventhubs -DartifactId=eventhubs-storm-spout -Dversion=0.9 -Dpackaging=jar`

- **If SQL**
- Download sqljdbc_4.0.2206.100_enu.tar.gz from [http://www.microsoft.com/en-us/download/details.aspx?id=11774](http://www.microsoft.com/en-us/download/details.aspx?id=11774)
- Extract and run the following command to import into local maven repository

	`mvn install:install-file -Dfile=sqljdbc4.jar -DgroupId=com.microsoft.sqlserver -DartifactId=sqljdbc4 -Dversion=4.0 -Dpackaging=jar`

- Create a new Maven project

	`mvn archetype:generate -DarchetypeArtifactId=maven-archetype-quickstart -DgroupId=com.hackathon.storm -DartifactId=EventHubExample -DinteractiveMode=false`

- Edit `pom.xml` add the following entries

    ```<build>
        <plugins>
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-compiler-plugin</artifactId>
                <version>2.3.2</version>
                <configuration>
                    <source>1.7</source>
                    <target>1.7</target>
                </configuration>
            </plugin>
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-shade-plugin</artifactId>
                <version>2.3</version>
                <configuration>
                    <transformers>
                        <transformer implementation="org.apache.maven.plugins.shade.resource.ApacheLicenseResourceTransformer">
                        </transformer>
                    </transformers>
                </configuration>
                <executions>
                    <execution>
                        <phase>package</phase>
                        <goals>
                            <goal>shade</goal>
                        </goals>
                    </execution>
                </executions>
            </plugin>
            <plugin>
                <groupId>org.codehaus.mojo</groupId>
                <artifactId>exec-maven-plugin</artifactId>
                <version>1.2.1</version>
                <executions>
                    <execution>
                        <goals>
                            <goal>exec</goal>
                        </goals>
                    </execution>
                </executions>
                <configuration>
                    <executable>java</executable>
                    <includeProjectDependencies>true</includeProjectDependencies>
                    <includePluginDependencies>false</includePluginDependencies>
                    <classpathScope>compile</classpathScope>
                    <mainClass>${storm.topology}</mainClass>
                </configuration>
            </plugin>
        </plugins>
        <resources>
            <resource>
                <directory>${basedir}/conf</directory>
                <filtering>false</filtering>
                <includes>
                    <include>Config.properties</include>
                </includes>
            </resource>
        </resources>
    </build>```

- Add the following dependencies to pom.xml

	```<dependencies>
    	<dependency>
      		<groupId>junit</groupId>
      		<artifactId>junit</artifactId>
      		<version>3.8.1</version>
      		<scope>test</scope>
    	</dependency>
      	<dependency>
          	<groupId>org.apache.storm</groupId>
          	<artifactId>storm-core</artifactId>
          	<version>0.9.2-incubating</version>
          	<!-- keep storm out of the jar-with-dependencies -->
          <scope>provided</scope>
     	</dependency>
      	<dependency>
          	<groupId>com.microsoft.eventhubs</groupId>
          	<artifactId>eventhubs-storm-spout</artifactId>
          	<version>0.9</version>
	    </dependency>
	    <dependency>
	   		<groupId>com.google.code.gson</groupId>
	        <artifactId>gson</artifactId>
          	<version>2.2.2</version>
      	</dependency>
      	<dependency>
          	<groupId>com.github.ptgoetz</groupId>
          	<artifactId>storm-hbase</artifactId>
          	<version>0.1.2</version>
      	</dependency>
      	<dependency>
          	<groupId>com.netflix.curator</groupId>
          	<artifactId>curator-framework</artifactId>
          	<version>1.3.3</version>
          	<exclusions>
            	<exclusion>
                	<groupId>log4j</groupId>
                  	<artifactId>log4j</artifactId>
              	</exclusion>
              	<exclusion>
                 	<groupId>org.slf4j</groupId>
                  	<artifactId>slf4j-log4j12</artifactId>
              	</exclusion>
          	</exclusions>
          	<scope>provided</scope>
      	</dependency>
      	<dependency>
          	<groupId>redis.clients</groupId>
          	<artifactId>jedis</artifactId>
          	<version>2.1.0</version>
      	</dependency>
      	<dependency>
          	<groupId>org.mockito</groupId>
          	<artifactId>mockito-all</artifactId>
          	<version>1.8.4</version>
     	</dependency>
      	<dependency>
          	<groupId>com.microsoft.sqlserver</groupId>
          	<artifactId>sqljdbc4</artifactId>
          	<version>4.0</version>
      	</dependency>
    </dependencies>

- **TODO: Add in settings file!!!**
- 
- Create a new package 

```
	com.hackathon.storm


- Add a new file ```ParseBolt.java``` and copy the following contents to the file.  This bolt will receive a JSON message from the Event Hub Spout and extract the values.  This will be placed on the STORM tuple stream for downstream processing.

```	
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
        	outputFieldsDeclarer.declareStream("energystream", 	new Fields("timestamp", "deviceid", "reading"));
    	}
	}

- Create a new file ```AugBolt.java``` and copy in the following code.  This will augment the stream by adding in the current timestamp.

```
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
	        int reading = tuple.getIntegerByField("reading");
	
	        DateFormat targetFormat = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss", Locale.ENGLISH);
	        String servertimestamp = targetFormat.format(new Date());
	
	        basicOutputCollector.emit("energystream", new Values(timestamp, deviceid, reading, servertimestamp));
	    }
	
	    @Override
	    public void declareOutputFields(OutputFieldsDeclarer outputFieldsDeclarer) {
        outputFieldsDeclarer.declareStream("energystream", new Fields	("timestamp", "deviceid", "reading", "servertimestamp"));
    	}
	}

-**IF SQL**

- Create a new file ```SqlStorageBolt.java``` and copy the following contents.  This will extract the data from the stream and insert a row into a MS SQL Database table.

```
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

-**IF REDIS**

- Add another file ```RedisStorageBolt.java``` and copy the following contents.  This will add a new key to redis (timestamp) and will populate the value as a JSON string representing the data on the tuple

	```
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
	
