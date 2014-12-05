# RIoT Labs #
## Energy Monitoring ##
**Lab 2**

**Energy Monitor**

Arduino Due

**Gateway**

Tessel?

**Event Hub**

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
  	</dependencies>```

- Create a new package 

	```com.hackathon.storm```

- Add a new file ```ParseBolt.java``` and copy the following contents to the file

	```package com.hackathon.storm;
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
	}```
