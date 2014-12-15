using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace businessrules.DeviceControl
{
    public class HeaterCommunication
    {
        private TopicClient _client;
        private SubscriptionClient _subClient;
        private MessagingFactory _factory;
        private NamespaceManager _namespaceMgr;
        private string _topicNameReceive;

        public HeaterCommunication()
        {
            var topicNameSend = "businessrulestofieldgateway";
            _topicNameReceive = "fieldgatewaytobusinessrules";
            _namespaceMgr = NamespaceManager.CreateFromConnectionString(CloudConfigurationManager.GetSetting("ServiceBusConnectionString"));
            _factory = MessagingFactory.CreateFromConnectionString(CloudConfigurationManager.GetSetting("ServiceBusConnectionString"));
            _client = _factory.CreateTopicClient(topicNameSend);
        }

        public HeaterStatus TurnOn(string correlationId)
        {
            return DoCommunication(correlationId, "on");
        }
        public HeaterStatus TurnOff(string correlationId)
        {
            return DoCommunication(correlationId, "off");
        }
        public HeaterStatus Query(string correlationId)
        {
            return DoCommunication(correlationId, "query");
        }


        private HeaterStatus DoCommunication(string correlationId, string action)
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
            var utf8String = "{ 'action' : '" + action + "' }";
            BrokeredMessage message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(utf8String)), true);
            message.CorrelationId = correlationId;

            return message;
        } 

        private HeaterStatus Parse(string reply)
        {
            var jObject = JObject.Parse(reply);

            if (jObject["heaterStatus"].Value<string>() == "ON")
                return HeaterStatus.ON;
            else if (jObject["heaterStatus"].Value<string>() == "OFF")
                return HeaterStatus.OFF;

            return HeaterStatus.UNKNOWN;

        }
    }
}
