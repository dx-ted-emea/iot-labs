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

namespace businessrules
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private TopicClient _client;
        private SubscriptionClient _subClient;

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

            var topicNameSend = "businessrulestofieldgateway";
            var topicNameReceive = "fieldgatewaytobusinessrules";
            var factory = MessagingFactory.CreateFromConnectionString(CloudConfigurationManager.GetSetting("ServiceBusConnectionString"));
            _client = factory.CreateTopicClient(topicNameSend);
            _subClient = factory.CreateSubscriptionClient(topicNameReceive, "all");

            while (true)
            {
                var sessionId = Guid.NewGuid().ToString("n");

                var heaterStatus = Query(sessionId);
                using (TemperatureReadingContext context = new TemperatureReadingContext(temperatureDbConnectionString))
                {
                    var tempReading = context.Readings.OrderByDescending(t=>t.StartTime).First();

                    if (tempReading.Temperature >= 22)
                    {
                        if (heaterStatus == HeaterStatus.ON)
                            TurnOff(sessionId);
                    }
                    else if (tempReading.Temperature <= 20)
                    {
                        if (heaterStatus == HeaterStatus.OFF)
                            TurnOn(sessionId);
                    }
                }
                await Task.Delay(30000);//temperature is recorded at a freshness hertz of 60 seconds, check twice as frequently 
            }
        }

        HeaterStatus TurnOn(string sessionId)
        {
            return HeaterCommunication(sessionId, "on");
        }
        HeaterStatus TurnOff(string sessionId)
        {
            return HeaterCommunication(sessionId, "off");
        }
        HeaterStatus Query(string sessionId)
        {
            return HeaterCommunication(sessionId, "query");
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

        private HeaterStatus HeaterCommunication(string sessionId, string action)
        {
            Trace.TraceInformation("Performing Heater Action: {0}", action);
            _client.Send(CreateMessage(sessionId, action));

            var receiveMessage = _subClient.Receive();
            string s = receiveMessage.GetBody<string>();

            Trace.TraceInformation("Heater Reports: {0}", s);
            return Parse(s);
        }

        static BrokeredMessage CreateMessage(string sessionId, string action)
        {
            var utf8String = "{ 'action' : '"+action+"' }";
            BrokeredMessage message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(utf8String)), true);
            message.SessionId = sessionId;
            message.MessageId = "Order_" + Guid.NewGuid().ToString().Substring(0, 5);
            return message;
        } 
    }
}
