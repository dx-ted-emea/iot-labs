using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TransportType = Microsoft.ServiceBus.Messaging.TransportType;


namespace IotEventHubBatchingWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("IotEventHubBatchingWorker is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("IotEventHubBatchingWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("IotEventHubBatchingWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("IotEventHubBatchingWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {

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

                    var messageBuffer = new List<string>();

                    var startTime = DateTime.UtcNow;

                    while (true)
                    {
                        try
                        {
                            //read the message
                            var message = receiver.Receive();

                            if (message == null)
                                continue;

                            var body = Encoding.UTF8.GetString(message.GetBytes());

                            if (body == null)
                                continue;

                            var currentTime = DateTime.UtcNow;

                            //add to the buffer
                            messageBuffer.Add(body);

                            //write out a file if a minute has passed and we have at least one message
                            if ((currentTime - startTime).TotalMinutes >= 5 && messageBuffer.Count >= 1)
                            {
                                var now = DateTime.Now;
                                var asString = String.Join("\n", messageBuffer);

                                //upload the blob
                                var blockBlob = container.GetBlockBlobReference(String.Format("{0}/{1}/{2}/message_{3}_{4}.log", now.Year, now.Month, now.Day, id, now.TimeOfDay));
                                blockBlob.UploadFromStream(new MemoryStream(Encoding.UTF8.GetBytes(asString)));

                                //clear the buffer
                                messageBuffer.Clear();

                                //start tracking anew
                                startTime = currentTime;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            //suppress for simplicity
                        }
                    }
                }, cancellationToken)).ToArray());

            }
        }
    }
}
