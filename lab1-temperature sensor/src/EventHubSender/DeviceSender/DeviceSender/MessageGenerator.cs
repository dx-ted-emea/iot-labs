using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceSender;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.IoTLabs.DeviceSender
{
    public class MessageGenerator
    {
        public MessageGenerator(DeviceSendingDetails deviceSendingDetails)
        {
            DeviceSendingDetails = deviceSendingDetails;
        }

        public DeviceSendingDetails DeviceSendingDetails { get; private set; }

        public void SendMessages(ConnectionDetails details)
        {
            var connectionString = String.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2};TransportType=Amqp",
                                                 details.ServiceBusNamespace,
                                                 details.SasPolicyName,
                                                 details.SasPolicyKey);
            var factory = MessagingFactory.CreateFromConnectionString(connectionString);
            var eventHubClient = factory.CreateEventHubClient(details.EventHubName);
            for (int i = 0; i < DeviceSendingDetails.IterationSeconds; i++)
            {
                SendDeviceEventStream(eventHubClient);
                Console.WriteLine("Messages fired onto the eventhub!");
                Thread.Sleep(DeviceSendingDetails.MillisecondDelay);
            }
        }

        private void SendDeviceEventStream(EventHubClient eventHubClient)
        {
            var allEvents = new List<EventData>();
            for (int i = 0; i < DeviceSendingDetails.NumberOfDevices; i++)
            {
                string deviceName = "device" + i;
                var rand = new Random();
                // set up the modifier to enable 
                float modifier = 1.0F;
                if (DeviceSendingDetails.FailureConditions.Any(device => device.FailedDeviceId == i))
                {
                    var deviceDetails = DeviceSendingDetails.FailureConditions.First(device => device.FailedDeviceId == i);
                    modifier = modifier += deviceDetails.FailedDeviceGradient;
                }
                var deviceValue = rand.Next((int) ((DeviceSendingDetails.TemperatureMin * modifier)*100),
                    (int) ((DeviceSendingDetails.TemperatureMax * modifier)*100));
                var deviceData = new DeviceData()
                {
                    deviceid = deviceName,
                    temperature = (deviceValue/100F),
                    timestamp = DateTime.UtcNow
                };
                var jsonDeviceDetail = JsonConvert.SerializeObject(deviceData);
                var encodedPayload = Encoding.UTF8.GetBytes(jsonDeviceDetail);
                var eventData = new EventData(encodedPayload)
                {
                    PartitionKey = "devices"
                };
                allEvents.Add(eventData);
            }
            eventHubClient.SendBatch(allEvents);
        }
    }

    public class DeviceData
    {
        public string deviceid { get; set; }
        public float temperature { get; set; }
        public DateTime timestamp { get; set; }
    }
}
