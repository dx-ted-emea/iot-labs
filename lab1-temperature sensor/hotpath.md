## Building a hot path analytics streaming capture facility ##

We'll be looking at simple pattern of Event Hub -> Streaming Analytics ->  SQLDB -> AzureML in this lab. 

The purpose is to be able to aggregate device readings over a 60 second window from event hub messages direct from the device or a gateway of some type. In order to begin we'll be creating a Windows Azure SQL Database called "temperaturedb" which will contain the aggregated values over a 60 period grouped by the device id.

When the database is created as per the earlier labs. Ensure that you enable a firewall rule so that we can execute some SQL and create the appropriate table.

![Adding a SQL Firewall Rule](images/Database%20Sql%20Firewall.png)

The following SQL can be used.

```sql
CREATE TABLE dbo.avgdevicereadings
(
	readingid INT IDENTITY PRIMARY KEY,
	starttime DATETIME not null,
	endtime DATETIME not null,
	deviceid VARCHAR(100) not null,
	temperature FLOAT not null,
	eventcount INT not null
)
```

Add a clustered index as each WASD table should have one.

```sql
CREATE CLUSTERED INDEX ix_avgdevicereadings_deviceid 
    ON dbo.avgdevicereadings (deviceid)
```

We're going to use the event hub to relay messages from the devices and consume them through Streaming Analytics, a new Microsoft Azure technology to enable Complex Event Processing (CEP). So we'll read from the Event Hub transform the on-the-wire messages using Streaming Analytics and place the aggregated outputs into a Windows Azure SQL Database.

We need to create an Event Hub now within a service bus namespace which will be used by the devices.After this has been created we need to create a Shared Access Policy which contain s set of privileges allowing Streaming Analytics to read messages from the Event Hub. In this case we've called the policy "streaming" and provided Manage, Send and Listen permissions.

![Create Shared Access Policy](images/Temperature%20Event%20hub.png)

Inevitably we'll be sending JSON messages from the device which look like this:

```javascript
{"device_id": "", "temperature": 21.2, "timestamp": "dd/mm/yyyy hh:MM:ss"}
```

To begin now we'll create a Streaming Analytics job using the Azure Portal:

![Create a Stream Analytics Job](images/Create%20Stream%20Analytics.png)

When this has been created we'll create a input which will allow Stream Analytics to read from the Event Hub.

![Create a Stream Analytics Input](images/Begin%20Add%20Input.png)

From there we can select whether we prefer to read from a Data Stream (continuously) or from Reference Data:

![Select input type](images/Add%20Input%202.png)

Next we can decide whether we want to read from Blob Storage or the Event Hub. In this case we'll read from the Event Hub we just created.

![Select source type](images/Add%20Input%203.png)

In the Event hub details page we know we need to configure the Event Hub. If we select the Event Hub we previously created in our subscription it will give us a list of the policy names to select from the dropdown list. In this case we'll choose the previously created policy called "streaming".

![Select source type](images/Add%20Input%204.png)

We'll then need to select the type of on-the-wire application and character encoding. At the time of writing Stream Analytics supports both UTF-8 encoded CSV messages or JSON messages. This means that when you write a query you can get the values from the input through the names in the JSON or oridinal position.

![Select source type](images/Add%20Input%205.png)

When this is complete you should the completed input screen like so (note the fact that Stream Analytics registers the connection to the Event Hub):

![Select source type](images/Add%20Input%206.png)

Now that the input source has been defined and configured we'll want to look at going through the same wizard-driven process to configure Stream Analytics to work with the output database that we created earlier.

![Select source type](images/Add%20Output%201.png)

From the dropdown you'll select the database from your subscription and acknowledge the database server name (which isn't postfixed by .database.windows.net). You'll need to enter the username and password you previously entered for the database server and select the correct database from the dropdown. In this example we created "temperaturedb" earlier on.

![Select source type](images/Add%20Output%202.png)

After both our inputs/outputs are configured we'll have to tell Stream Analytics to do something. In this case we're going to need to track every message over a 1 minute window for a particular device and count the number of events and average temperature across that minute. In order to do the aggregation we'll navigate to the "query" tab and enter the following which will do the transformation.

```sql
SELECT DateAdd(minute,-1,System.TimeStamp) as starttime, system.TimeStamp as endtime, deviceid, Avg(temperature) as temperature, Count(*) as eventcount 
FROM input
GROUP BY TumblingWindow(minute, 1), DeviceId
```

This will ensure that all deviceid details will be aggregated on a 1 minute basis. In this instance there may be something in the order of hundreds of messages in the eventcount field in a particular row. The temperature of sensor will be averaged over that period.

Stream Analytics will run in the background now when started. In order to start it up we need to select the configure tab. On pressing the start button as per the image below we'll see a dialogue. We can select any date before the date that we start acquiring messages

![Select source type](images/start%20stream%20analytics%20job.png)

## Setting up the event source  ##
Now that we've set up the capture and aggregate facility with Stream Analytics we need to setup the event source. There is a supplied C# program that we can leverage which will enable us to send messages for testing purposes and also send anomalous messages.

In order to send messages you need to open up the DeviceSender project in Visual Studio and build the project. Navigate to the relevant directory where the executable resides, open a command prompt and enter the following.

`DeviceSender devicestreaming-ns iottemperatures streaming RcHsE6J0I2C5id8KKeyk7OdPtnaI/pQImkvQQKSZnSs=`

The test harness generates a set of eventhub messages simulating devices issuing "temperature pings". 

If you look in the Program.cs file you will see the following structure which enables the sending of messages per a set of rules. A description of this is below and can be customised to generate the correct ruleset.

```csharp
var deviceDetails = new DeviceSendingDetails()
{
	FailureConditions = new[] {new FailedDeviceSettings(3, 0.1F)},
    IterationSeconds = 30,
    NumberOfDevices = 50,
    TemperatureMax = 28.9F,
    TemperatureMin = 19.6F,
    MillisecondDelay = 1000
};
```



- **NumberOfDevices** this is the number of devices that are simulated each device is called device1, device2 ... devicen


- **IterationSeconds** this is the number of seconds over which the messages will be sent. In this case this will be sent over 30 seconds 


- **TemperatureMax** The maximum temperature that the message will be bounded by


- **TemperatureMin** The minimum temperature that the message will be bounded by


- **MillsecondDelay** the amount of delay between message sends in this case 1 second 


- **FailureConditions** An array in the form of a device id and a temperature skew in degrees

Most of the generated messages will average to 24 degree or thereabouts in Stream Analytics.

For this lab, **FailureConditions** represents an important part of the design. We'll be using this value to inform a machine learning algorithm whether the device is failing by looking at a device which is generating a slight skew over time. While our particular usage of Machine Learning will not look at the temperature but will treat the device anomaly as an outlier this could be extended to look at this aspect. 

Note that the FailureCondition is a factor of the simulator only, and that actual failures of temperature readings is the ML input for later consumption.

## Following on

Continue onto building an Energy monitor and introduce the idea of a Cloud Gateway in [Lab2](../lab2-energy monitor/) or visualise the results of this lab in [Lab4](../lab4-visualisations/)