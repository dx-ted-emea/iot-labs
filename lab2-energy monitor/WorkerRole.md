# Hadoop #

## Event Hub to Blob Storage ##

To ensure data is available to Hadoop we need to collate messages over time and make them write them to blob storage.

To keep things simple we will create a Azure Worker role to read messages from a explicit Consumer Group, and write each to blob storage as a separate blob.  In a real-world scenario we would normally build up a record locally and to upload this at the end of the day.  We would also build in multiple blocks into the blob and implement a retry mechanism on failure.  

### Create Consumer Group ###

In order for different consumers to read from an event hub simultaneously we need to define a Consumer Group for each consumer.  The following outlines the steps required to complete this;

**Pre-requisites**

- Azure PowerShell installed
- A copy of Microsoft.ServiceBus.dll - this can be gathered by using the nuget commandline interface

**Add Consumer Group**

- Import the ServiceBus dll

	`Import-Module <path>/Microsoft.ServiceBus.dll`

- Login to Azure

	`Add-AzureAccount`

- Select Azure Subscription (if appropriate)

	`Select-AzureSubscription  <SubscriptionName>`
- Execute the following lines to add the Consumer Group to the target Event Hub

```powershell
	$NamespaceManager = [Microsoft.ServiceBus.NamespaceManager]::CreateFromConnectionString(<ConnectionString>);
	$ConsumerGroupDescription = New-Object -TypeName Microsoft.ServiceBus.Messaging.ConsumerGroupDescription -ArgumentList <EventHubName>, <ConsumerGroupName> 
	$ConsumerGroupDescription.UserMetadata = $ConsumerGroupUserMetadata 
	$NamespaceManager.CreateConsumerGroupIfNotExists($ConsumerGroupDescription);
``` 
- You can view whether the consumer group was created successfully by executing the command

	`$NamespaceManager.GetConsumerGroups(<EventHubName>)`

** Worker Role **

In Visual Studio 2013 create a new Azure Cloud Service project

- Add the following entries to app.config (under <appSettings>)

```xml
    <add key="ServiceBus.ConnectionString" value="<ConnectionString>" />
    <add key="ServiceBus.Path" value="<EventHubName>" />
    <add key="ServiceBus.ConsumerGroup" value="<ConsumerGroup>" />
    <add key="Storage.ConnectionString" value="<BlobStorageConnectionString>" />
    <add key="Storage.Container" value="<BlobStorageContainer>" />
```

- Replace all the code in `WorkerRole.cs` in the method `RunAsync`.  This will spawn threads to read from each partition in the Event Hub.  Any read message will be uploaded to Azure Blob Storage as a single blob.

```c#
    //get a handle on the consumer group for the event hub we want to read from
    var factory = MessagingFactory.CreateFromConnectionString(ConfigurationManager.AppSettings["ServiceBus.ConnectionString"] + ";TransportType=Amqp");
    var client = factory.CreateEventHubClient(ConfigurationManager.AppSettings["ServiceBus.Path"]);
    var group = client.GetConsumerGroup(ConfigurationManager.AppSettings["ServiceBus.ConsumerGroup"]);

    //get a handle on the container we want to write blobs to
    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["Storage.ConnectionString"]);
    var blobClient = storageAccount.CreateCloudBlobClient();
    var container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["Storage.Container"]);
    container.CreateIfNotExists();

    while (!cancellationToken.IsCancellationRequested)
    {
        Task.WaitAll(client.GetRuntimeInformation().PartitionIds.Select(id => Task.Run(() =>
        {
            var receiver = @group.CreateReceiver(id);

            while (true)
            {
                try
                {
                    //read the message
                    var message = receiver.Receive();
                    var body = Encoding.UTF8.GetString(message.GetBytes());

                    //upload a blob
                    var now = DateTime.Now;
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(String.Format("{0}/{1}/{2}/message_{3}_{4}.log", now.Year, now.Month, now.Day, id, now.TimeOfDay));
                    blockBlob.UploadFromStream(new MemoryStream(Encoding.UTF8.GetBytes(body)));
                }
                catch
                {
                    //suppress for simplicity
                }
            }
        }, cancellationToken)).ToArray());

    }
 ```      
