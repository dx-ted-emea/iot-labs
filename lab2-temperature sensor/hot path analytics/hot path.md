## Building a hot path analytics streaming capture facility ##

We'll be looking at simple pattern of Event Hub -> Streaming Analytics ->  SQLDB -> AzureML in this lab. 

The purpose is to be able to aggregate device readings over a 60 second window from event hub messages direct from the device or a gateway of some type. In order to begin we'll be creating a Windows Azure SQL Database called "temperaturedb" which will contain the aggregated values over a 60 period grouped by the device id.

When the database is created as per the earlier labs. Ensure that you enable a firewall rule so that we can execute some SQL and create the appropriate table.

![Adding a SQL Firewall Rule](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Database%20Sql%20Firewall.png)

The following SQL can be used.

```sql
CREATE TABLE dbo.avgdevicereadings
(
	starttime DATETIME not null,
	endtime DATETIME not null,
	deviceid VARCHAR not null,
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

![Create Shared Access Policy](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Temperature%20Event%20hub.png)

Inevitably we'll be sending JSON messages from the device which look like this:

```javascript
{"device_id": "", "temperature": 21.2, "timestamp": "dd/mm/yyyy hh:MM:ss"}
```

To begin now we'll create a Streaming Analytics job using the Azure Portal:

![Create a Stream Analytics Job](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Create%20Stream%20Analytics.png)

When this has been created we'll create a input which will allow Stream Analytics to read from the Event Hub.

![Create a Stream Analytics Input](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Begin%20Add%20Input.png)

From there we can select whether we prefer to read from a Data Stream (continuously) or from Reference Data:

![Select input type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Input%202.png)

Next we can decide whether we want to read from Blob Storage or the Event Hub. In this case we'll read from the Event Hub we just created.

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Input%203.png)

In the Event hub details page we know we need to configure the Event Hub. If we select the Event Hub we previously created in our subscription it will give us a list of the policy names to select from the dropdown list. In this case we'll choose the previously created policy called "streaming".

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Input%204.png)

We'll then need to select the type of on-the-wire application and character encoding. At the time of writing Stream Analytics supports both UTF-8 encoded CSV messages or JSON messages. This means that when you write a query you can get the values from the input through the names in the JSON or oridinal position.

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Input%205.png)

When this is complete you should the completed input screen like so (note the fact that Stream Analytics registers the connection to the Event Hub):

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Input%206.png)

Now that the input source has been defined and configured we'll want to look at going through the same wizard-driven process to configure Stream Analytics to work with the output database that we created earlier.

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Output%201.png)

From the dropdown you'll select the database from your subscription and acknowledge the database server name (which isn't postfixed by .database.windows.net). You'll need to enter the username and password you previously entered for the database server and select the correct database from the dropdown. In this example we created "temperaturedb" earlier on.

![Select source type](https://elastastorage.blob.core.windows.net/mdimages/Hot%20Path%20Images/streaming%20analytics/Add%20Output%202.png)

After both our inputs/outputs are configured we'll have to tell Stream Analytics to do something. In this case we're going to need to track every message over a 1 minute window for a particular device and count the number of events and average temperature across that minute. In order to do the aggregation we'll navigate to the "query" tab and enter the following which will do the transformation.

```sql
SELECT DateAdd(minute,-1,System.TimeStamp) as starttime, system.TimeStamp as endtime, deviceid, Avg(temperature) as temperature, Count(*) as eventcount 
FROM input
GROUP BY TumblingWindow(minute, 1), DeviceId
```

This will ensure that all deviceid details will be aggregated on a 1 minute basis. In this instance there may be something in the order of hundreds of messages in the eventcount field in a particular row. The temperature of sensor will be averaged over that period.

Stream Analytics will run in the background now when started. In order to start it up we need to select the configure tab. On pressing the start button as per the image below we'll see a dialogue. We can select any date before the date that we start acquiring messages

![Select source type](start%20stream%20analytics%20job.png)

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

In order proceed we'll need to setup and AzureML namespace. To do this you need to be logged into the Azure portal and have also enabled AzureML since it's still in preview. In order to work out how to do this follow the instructions in the following link.

[Enable AzureML as a preview feature](http://azure.microsoft.com/en-gb/services/preview/)

To begin create a new namespace using the **New** button at the bottom of the portal screen.

![Create an AzureML namespace](create%20azure%20ml%20namespace.png)

You should notice that the owner of the "workspace" is the logged in user. You'll also notice that as this is still in preview it is only available in **South Central US** so you'll need a storage account in that location. The Creation process will give you the option of creating one if one doesn't exist. After creating it you should see it running under the **machine learning** tab.

![Running machine learning workspace](running%20machine%20learning.png)

We'll be creating a new **experiment** now which will allow us to create a new ML service. To begin we'll click on the **Open in ML Studio** button at the bottom of the portal. 

![Create a new experiment](ml%20studio%20create%20new%20experiment.png)

Now we can upload a dataset containing devices and their respective temperatures for each minute time period. We won't be analysing this in a time-sensitive manner so the only relevant data point in this case will be temperature. The dataset is hotpath.tsv and can be found in the current directory.

![Upload a dataset](add%20a%20dataset.png)

We'll now see this on the **Saved Datasets** section of the workspace and can drag this across onto a workspace. It has been named IoT Temperature Data but look for the name you gave to the file.

You can now consume the dataset and project columns out 

![Experiment with projected columns](experiment%20with%20projected%20columns.png)

To get to this we will need to drag **Data Transformation/Project Columns** from the left hand menu and then select properties and include only temperature.

![Select temperature column](include%20columns%20in%20projection.png)

We'll be looking at at a technique called K-Means Clustering which will spot the anomalies and assign them to clusters. It will automatically try and pick out the number of clusters from the trained dataset we are presenting it with. In a normal machine experiment we would normally use only a %'age of data to **train** our K-Means clustering model but in this case we'll use 100% of our dataset and see if it picks out the skewed device data in a separate cluster. 

![Completed clustering model](complete%20model.png)

In order to complete the model drag **Machine Learning/Initialize Model/Clustering/K-Means Clustering** from the left hand menu then connect this to **Train Clustering Model** which can be dragged from **Machine Learning/Train/Train Clustering Model** and finally **Machine Learning/Score/Assign To Clusters** follow the other connections in the image from **Project Columns**. It's important to customise the **Train Clustering Model** and **Assign To Clusters** by selecting select columns and choosing temperature as you did in the previous step **Project Columns**

We can now select **Run** from the bottom menu.

We can now right-click on the output of the Assign To Clusters step and choose **Visualize** from the context menu. You should see a visualisation similar to that of below which shows that all of the normal values have been correctly assessed to be in **Cluster 0** and the two skewed values in **Cluster 1**

![Visualizing clusters](visualise%20clusters.png)

We have no further use of our trained model so we can save this to ensure that we don't need to calculate this every time we want to predict whether a device data point is in Cluster 0 or Cluster 1. To do this we can right-click on the left output node of the Train Clustering Model and select **save trained model** from the context menu.

![Save trained model](save%20trained%20model.png)

Enter the correct information in the dialog.

![Save trained model dialog](save%20trained%20model%dialog.png)

We can now remove the K-Means Clustering and Train Clustering Model step and use our trained model. This should be available to us from the left hand menu as per the diagram. We can simply wire this up to the **Assign To Clusters** along with Project Columns as per the image below.

![Final model with trained dataset](final%20model%20with%20trained%20dataset.png)

The final step now is to expose this model so that it can be consumed from a service which will be able to pass the data points to it every minute. 

Now we can set both the published inputs and outputs by clicking on the right hand input of **Assign To Clusters** and selecting "Set as Published Input". Similarly we can right click on the output and select "Set as Published Output". Now we just need to press **Run** on the bottom menu and when this completes we should have a new button available to us **PUBLISH WEB SERVICE**

This should only take a few seconds to complete and will take us to a new page which will show us how we can consume the web service to send single temperature points to determine the clusters.

You should be able to see a web service in the ML Studio portal now.

![The new web service](web%20service.png)

Clicking on the web service will reveal details of how to invoke them using either C#, Python or R. We should be able to lift the entire code section for C# and add this to a worker role which will process a set of data points returned from a SQL query that has been processed by Stream Analytics. Each row temperature can then be sent to the web service after being requested on every minute on a timer.

To check whether the web service is processing and giving us the correct outputs we can click on the **test** link which will give us a dialog enabling us to enter a temperature value.

![Test a temperature prediction cluster](predict%20and%20test%20with%20ml.png)
