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
