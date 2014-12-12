# IoT Smart home Hackathon Lab #

## Lab 1 Real Time processing ##

Event Hubs is a highly scalable publish-subscribe ingestor that can intake millions of events per second so that you can process and analyse the massive amounts of data produced by your connected devices and applications. Once collected into Event Hubs, you can transform and store data using any real-time analytics provider or with batching/storage adapters.  The Event Hub will recieve json enconded messages from the Gateway and will be used for both real-time processing via storm and batch based processing via HDInsight/Hadoop.

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

```xml
	<build>
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
    </build>
```

- Add the following dependencies to pom.xml

```xml
	<dependencies>
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
```
- Create a new directory `conf` and add a file `Config.properties` with the following content

```properties
	#Event hub configuration
	eventhubspout.username = **<username>**
	eventhubspout.password = **<password>**
	eventhubspout.namespace =**<namespace>**
	eventhubspout.entitypath = **<eventhub>**
	eventhubspout.partitions.count = 16
	eventhubspout.checkpoint.interval = 1
	eventhub.receiver.credits = 10
	
	#IF Redis
	redis.host = **<redisHost>**
	redis.port = **<port>**
	redis.password = **<password>**
	redis.ttl = 1024
	
	#If SQL
	sql.connection = jdbc:sqlserver://**<sql server>**:1433;database=**<database>**;user=**<username>**;password=**<password>**;encrypt=true;hostNameInCertificate=*.database.windows.net;loginTimeout=30;
```
 
- Create a new package 


	`com.hackathon.storm`


- Add a new file `ParseBolt.java` and copy the following contents to the file.  This bolt will receive a JSON message from the Event Hub Spout and extract the values.  This will be placed on the STORM tuple stream for downstream processing.

```java
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
```

- Create a new file `AugBolt.java` and copy in the following code.  This will augment the stream by adding in the current timestamp.

```java
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
```
-**IF SQL**

- Create a new file `SqlStorageBolt.java` and copy the following contents.  This will extract the data from the stream and insert a row into a MS SQL Database table.

```java
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
```

-**IF REDIS**

- Add another file `RedisStorageBolt.java` and copy the following contents.  This will add a new key to redis (timestamp) and will populate the value as a JSON string representing the data on the tuple

```java
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
```
	
- If SQL
- Add an additional file to configure the topology `EnergyReaderSql.java` 

```java
	package com.hackathon.storm;
    
    import backtype.storm.Config;
    import backtype.storm.LocalCluster;
    import backtype.storm.StormSubmitter;
    import backtype.storm.generated.StormTopology;
    import backtype.storm.topology.TopologyBuilder;
    import backtype.storm.tuple.Fields;
    import com.microsoft.eventhubs.spout.EventHubSpout;
    import com.microsoft.eventhubs.spout.EventHubSpoutConfig;
    
    import java.util.Properties;
    
    public class EnergyReaderSql {
        protected EventHubSpoutConfig spoutConfig;
        protected int numWorkers;
    
        // Reads the configuration information for the Event Hub spout
        protected void readEHConfig(String[] args) throws Exception {
    
            Properties properties = new Properties();
            properties.load(this.getClass().getClassLoader().getResourceAsStream("Config.properties"));
    
            String username = properties.getProperty("eventhubspout.username");
            String password = properties.getProperty("eventhubspout.password");
            String namespaceName = properties.getProperty("eventhubspout.namespace");
            String entityPath = properties.getProperty("eventhubspout.entitypath");
    
            String zkEndpointAddress = properties.getProperty("zookeeper.connectionstring");
            int partitionCount = Integer.parseInt(properties.getProperty("eventhubspout.partitions.count"));
            int checkpointIntervalInSeconds = Integer.parseInt(properties.getProperty("eventhubspout.checkpoint.interval"));
            int receiverCredits = Integer.parseInt(properties.getProperty("eventhub.receiver.credits"));
            System.out.println("Eventhub spout config: ");
            System.out.println("  partition count: " + partitionCount);
            System.out.println("  checkpoint interval: " + checkpointIntervalInSeconds);
            System.out.println("  receiver credits: " + receiverCredits);
            spoutConfig = new EventHubSpoutConfig(username, password,
                    namespaceName, entityPath, partitionCount, zkEndpointAddress,
                    checkpointIntervalInSeconds, receiverCredits);
    
            //set the number of workers to be the same as partition number.
            //the idea is to have a spout and a partial count bolt co-exist in one
            //worker to avoid shuffling messages across workers in storm cluster.
            numWorkers = spoutConfig.getPartitionCount();
    
            if(args.length > 0) {
                //set topology name so that sample Trident topology can use it as stream name.
                spoutConfig.setTopologyName(args[0]);
            }
        }
    
        // Create the spout using the configuration
        protected EventHubSpout createEventHubSpout() {
            EventHubSpout eventHubSpout = new EventHubSpout(spoutConfig);
            return eventHubSpout;
        }
    
        // Build the topology
        protected StormTopology buildTopology(EventHubSpout eventHubSpout) {
            TopologyBuilder topologyBuilder = new TopologyBuilder();
            // Name the spout 'EventHubsSpout', and set it to create
            // as many as we have partition counts in the config file
            topologyBuilder.setSpout("EventHub", eventHubSpout, spoutConfig.getPartitionCount())
                    .setNumTasks(spoutConfig.getPartitionCount());
            // Create the parser bolt, which subscribes to the stream from EventHub
            topologyBuilder.setBolt("ParseBolt", new ParseBolt(), spoutConfig.getPartitionCount())
                    .localOrShuffleGrouping("EventHub").setNumTasks(spoutConfig.getPartitionCount());
    
            topologyBuilder.setBolt("AugBolt", new AugBolt(), spoutConfig.getPartitionCount())
                    .fieldsGrouping("ParseBolt", "energystream", new Fields("timestamp", "deviceid", "reading")).setNumTasks(spoutConfig.getPartitionCount());
    
            // Create the dashboard bolt, which subscribes to the stream from Parser
            topologyBuilder.setBolt("SqlStorageBolt", new SqlStorageBolt(), spoutConfig.getPartitionCount())
                    .fieldsGrouping("AugBolt", "energystream", new Fields("timestamp", "deviceid", "reading", "servertimestamp")).setNumTasks(spoutConfig.getPartitionCount());
    
            return topologyBuilder.createTopology();
        }
    
        protected void submitTopology(String[] args, StormTopology topology, Config config) throws Exception {
            // Config config = new Config();
            config.setDebug(false);
    
            //Enable metrics
            config.registerMetricsConsumer(backtype.storm.metric.LoggingMetricsConsumer.class, 1);
    
            // Is this running locally, or on an HDInsight cluster?
            if (args != null && args.length > 0) {
                config.setNumWorkers(numWorkers);
                StormSubmitter.submitTopology(args[0], config, topology);
            } else {
                config.setMaxTaskParallelism(2);
    
                LocalCluster localCluster = new LocalCluster();
                localCluster.submitTopology("test", config, topology);
    
                Thread.sleep(5000000);
    
                localCluster.shutdown();
            }
        }
    
        // Loads the configuration, creates the spout, builds the topology,
        // and then submits it
        protected void runScenario(String[] args) throws Exception{
            readEHConfig(args);
            Config config = new Config();
    
            EventHubSpout eventHubSpout = createEventHubSpout();
            StormTopology topology = buildTopology(eventHubSpout);
            submitTopology(args, topology, config);
        }
    
        public static void main(String[] args) throws Exception {
            EnergyReaderSql scenario = new EnergyReaderSql();
            scenario.runScenario(args);
        }
    }
```

- If Redis
- Add an additional file to configure the topology `EnergyReaderRedis.java`

```java
	package com.hackathon.storm;
    
        import backtype.storm.Config;
        import backtype.storm.LocalCluster;
        import backtype.storm.StormSubmitter;
        import backtype.storm.generated.StormTopology;
        import backtype.storm.topology.TopologyBuilder;
        import backtype.storm.tuple.Fields;
        import com.microsoft.eventhubs.spout.EventHubSpout;
        import com.microsoft.eventhubs.spout.EventHubSpoutConfig;
    
        import java.util.Properties;
    
        /**
         * Created by david on 02/12/14.
         */
        public class EnergyReaderRedis {
            protected EventHubSpoutConfig spoutConfig;
            protected int numWorkers;
    
            // Reads the configuration information for the Event Hub spout
            protected void readEHConfig(String[] args) throws Exception {
    
                Properties properties = new Properties();
                properties.load(this.getClass().getClassLoader().getResourceAsStream("Config.properties"));
    
    
                String username = properties.getProperty("eventhubspout.username");
                String password = properties.getProperty("eventhubspout.password");
                String namespaceName = properties.getProperty("eventhubspout.namespace");
                String entityPath = properties.getProperty("eventhubspout.entitypath");
                String zkEndpointAddress = properties.getProperty("zookeeper.connectionstring");
                int partitionCount = Integer.parseInt(properties.getProperty("eventhubspout.partitions.count"));
                int checkpointIntervalInSeconds = Integer.parseInt(properties.getProperty("eventhubspout.checkpoint.interval"));
                int receiverCredits = Integer.parseInt(properties.getProperty("eventhub.receiver.credits"));
                System.out.println("Eventhub spout config: ");
                System.out.println("  partition count: " + partitionCount);
                System.out.println("  checkpoint interval: " + checkpointIntervalInSeconds);
                System.out.println("  receiver credits: " + receiverCredits);
                spoutConfig = new EventHubSpoutConfig(username, password,
                        namespaceName, entityPath, partitionCount, zkEndpointAddress,
                        checkpointIntervalInSeconds, receiverCredits);
    
                //set the number of workers to be the same as partition number.
                //the idea is to have a spout and a partial count bolt co-exist in one
                //worker to avoid shuffling messages across workers in storm cluster.
                numWorkers = spoutConfig.getPartitionCount();
    
                if(args.length > 0) {
                    //set topology name so that sample Trident topology can use it as stream name.
                    spoutConfig.setTopologyName(args[0]);
                }
            }
    
            // Create the spout using the configuration
            protected EventHubSpout createEventHubSpout() {
                EventHubSpout eventHubSpout = new EventHubSpout(spoutConfig);
                return eventHubSpout;
            }
    
            // Build the topology
            protected StormTopology buildTopology(EventHubSpout eventHubSpout) {
                TopologyBuilder topologyBuilder = new TopologyBuilder();
                // Name the spout 'EventHubsSpout', and set it to create
                // as many as we have partition counts in the config file
                topologyBuilder.setSpout("EventHub", eventHubSpout, spoutConfig.getPartitionCount())
                        .setNumTasks(spoutConfig.getPartitionCount());
                // Create the parser bolt, which subscribes to the stream from EventHub
                topologyBuilder.setBolt("ParseBolt", new ParseBolt(), spoutConfig.getPartitionCount())
                        .localOrShuffleGrouping("EventHub").setNumTasks(spoutConfig.getPartitionCount());
    
                topologyBuilder.setBolt("AugBolt", new AugBolt(), spoutConfig.getPartitionCount())
                        .fieldsGrouping("ParseBolt", "energystream", new Fields("timestamp", "deviceid", "reading")).setNumTasks(spoutConfig.getPartitionCount());
    
                // Create the dashboard bolt, which subscribes to the stream from Parser
                topologyBuilder.setBolt("RedisStorageBolt", new RedisStorageBolt(), spoutConfig.getPartitionCount())
                        .fieldsGrouping("AugBolt", "energystream", new Fields("timestamp", "deviceid", "reading", "servertimestamp")).setNumTasks(spoutConfig.getPartitionCount());
    
                 return topologyBuilder.createTopology();
            }
    
            protected void submitTopology(String[] args, StormTopology topology, Config config) throws Exception {
                // Config config = new Config();
                config.setDebug(false);
    
                //Enable metrics
                config.registerMetricsConsumer(backtype.storm.metric.LoggingMetricsConsumer.class, 1);
    
                // Is this running locally, or on an HDInsight cluster?
                if (args != null && args.length > 0) {
                    config.setNumWorkers(numWorkers);
                    StormSubmitter.submitTopology(args[0], config, topology);
                } else {
                    config.setMaxTaskParallelism(2);
    
                    LocalCluster localCluster = new LocalCluster();
                    localCluster.submitTopology("test", config, topology);
    
                    Thread.sleep(5000000);
    
                    localCluster.shutdown();
                }
            }
    
            // Loads the configuration, creates the spout, builds the topology,
            // and then submits it
            protected void runScenario(String[] args) throws Exception{
                readEHConfig(args);
                Config config = new Config();
    
                EventHubSpout eventHubSpout = createEventHubSpout();
                StormTopology topology = buildTopology(eventHubSpout);
                submitTopology(args, topology, config);
            }
    
            public static void main(String[] args) throws Exception {
                EnergyReaderRedis scenario = new EnergyReaderRedis();
                scenario.runScenario(args);
            }
        }
```

Running the topology
-

Once the example is complete the topology can be deployed to the storm cluster.

First build the jar.  Run a maven build.

	mvn package

-**IF REDIS**
Copy the resulting jar to the Storm cluster.  Run the following command to deploy the topology.

	cd %STORM_HOME%
	.\bin\storm jar EventHubExample-1.0-SNAPSHOT.jar com.hackathon.storm.EnergyReaderRedis EnergyReaderRedis

-**IF SQL**
Copy the resulting jar to the Storm cluster.  Run the following command to deploy the topology.

	cd %STORM_HOME%
	.\bin\storm jar EventHubExample-1.0-SNAPSHOT.jar com.hackathon.storm.EnergyReaderSql EnergyReaderSql

The topology status can be viewed on the Storm cluster by navigating to URL [http://headnodehost:8772/](http://headnodehost:8772/)
