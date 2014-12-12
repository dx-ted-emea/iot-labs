using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using businessrules.DataAccess;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.ServiceBus;

namespace businessrules
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private TopicClient _client;
        private SubscriptionClient _subClient;
        private MessagingFactory _factory;
        private NamespaceManager _namespaceMgr;
        private string _topicNameReceive;

        public override void Run()
        {
            Trace.TraceInformation("businessrules is running");

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

            Trace.TraceInformation("businessrules has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("businessrules is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("businessrules has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var temperatureDbConnectionString = CloudConfigurationManager.GetSetting("TemperatureDbConnectionString");
            var notificationEndpoint = CloudConfigurationManager.GetSetting("NotificationUri");

            var topicNameSend = "businessrulestofieldgateway";
            _topicNameReceive = "fieldgatewaytobusinessrules";
            _namespaceMgr = NamespaceManager.CreateFromConnectionString(CloudConfigurationManager.GetSetting("ServiceBusConnectionString"));
            _factory = MessagingFactory.CreateFromConnectionString(CloudConfigurationManager.GetSetting("ServiceBusConnectionString"));
            _client = _factory.CreateTopicClient(topicNameSend);

            while (true)
            {
                var correlationId = Guid.NewGuid().ToString("n");

                var heaterStatus = Query(correlationId);
                using (TemperatureReadingContext context = new TemperatureReadingContext(temperatureDbConnectionString))
                {
                    //get the most recent entry
                    var tempReading = context.Readings.OrderByDescending(t=>t.StartTime).First();

                    if (tempReading.Temperature >= 22)
                    {
                        if (heaterStatus == HeaterStatus.ON)
                        {
                            TurnOff(correlationId);
                            NotifyWebUi(notificationEndpoint, false);
                        }
                    }
                    else if (tempReading.Temperature <= 20)
                    {
                        if (heaterStatus == HeaterStatus.OFF)
                        {
                            TurnOn(correlationId);
                            NotifyWebUi(notificationEndpoint, true);
                        }
                    }
                }
                await Task.Delay(30000);//temperature is recorded at a freshness hertz of 60 seconds, check twice as frequently 
            }
        }


        HeaterStatus TurnOn(string correlationId)
        {
            return HeaterCommunication(correlationId, "on");
        }
        HeaterStatus TurnOff(string correlationId)
        {
            return HeaterCommunication(correlationId, "off");
        }
        HeaterStatus Query(string correlationId)
        {
            return HeaterCommunication(correlationId, "query");
        }

        HeaterStatus Parse(string reply)
        {
            var jObject = JObject.Parse(reply);

            if (jObject["heaterStatus"].Value<string>() == "ON")
                return HeaterStatus.ON;
            else if (jObject["heaterStatus"].Value<string>() == "OFF")
                return HeaterStatus.OFF;

            return HeaterStatus.UNKNOWN;
            
        }

        private HeaterStatus HeaterCommunication(string correlationId, string action)
        {
            var subscriptionDesc = new SubscriptionDescription(_topicNameReceive, correlationId);
            subscriptionDesc.DefaultMessageTimeToLive = TimeSpan.FromSeconds(30);
            _namespaceMgr.CreateSubscription(subscriptionDesc, new CorrelationFilter(correlationId));

            Trace.TraceInformation("Performing Heater Action: {0}", action);
            _client.Send(CreateMessage(correlationId, action));

            var receiveClient = _factory.CreateSubscriptionClient(_topicNameReceive, correlationId, ReceiveMode.ReceiveAndDelete);
            var receiveMessage = receiveClient.Receive();

            string s = receiveMessage.GetBody<string>();
            Trace.TraceInformation("Heater Reports: {0}", s);

            _namespaceMgr.DeleteSubscription(_topicNameReceive, correlationId);

            return Parse(s);
        }

        static BrokeredMessage CreateMessage(string correlationId, string action)
        {
            var utf8String = "{ 'action' : '"+action+"' }";
            BrokeredMessage message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(utf8String)), true);
            message.CorrelationId = correlationId;

            return message;
        } 
        private async Task NotifyWebUi(string notificationEndpoint, bool isTurnedOn)
        {
            var jsonString = JObject.FromObject(new { EventTime = DateTime.Now, IsTurnedOn = isTurnedOn }).ToString();
            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                await client.PostAsync(notificationEndpoint, content);
            }
        }
    }
}
