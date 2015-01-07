using businessrules_fieldgateway_simulator_console.Helpers;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace businessrules_fieldgateway_simulator_console
{
    /// <summary>
    /// Use this simulator to make the BusinessRules self contained and isolated from the Field Gateway.
    /// You may overwrite the default value of the SB connectionstring in code or at runtime
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var sendTopic = "fieldgatewaytobusinessrules";
            var receiveTopic = "businessrulestofieldgateway";

            var defaultUri = "Endpoint=sb://iotlabs.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=v2vGRS15kRoeZhb++6M77BS7IplSXCdvMfZnfrwP97M=";

            if (ConsoleHelper.UserYInputPrompt("Override ServiceBus Connection String? Y/n"))
            {
                defaultUri = ConsoleHelper.UserInputPrompt("Enter ServiceBus Connection String.");
            }

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(defaultUri, receiveTopic, "all", ReceiveMode.PeekLock);
            var topicClient = TopicClient.CreateFromConnectionString(defaultUri, sendTopic);

            ConsoleHelper.WriteTitle("Simulating the Field Gateway");
            while (true)
            {
                Console.WriteLine("Waiting for messages");
                var receiveMessage = subscriptionClient.Receive();

                if (receiveMessage == null) continue;
                    
                Stream ms = receiveMessage.GetBody<Stream>();
                var s = new StreamReader(ms).ReadToEnd();
                var correlationId = receiveMessage.CorrelationId;
                receiveMessage.Complete();

                Console.WriteLine("Business Rules (instance {1}) Commands: {0}", s, correlationId);

                var status = ConsoleHelper.UserChoicePrompt("Enter the simulated Heater Status",
                    new[] { "ON", "OFF", "ERR" });

                var message = CreateMessage(correlationId, status);
                topicClient.Send(message);

                ConsoleHelper.WriteSuccess("Replied with " + status);
            }
        }
        static BrokeredMessage CreateMessage(string correlationId, string status)
        {
            var utf8String = "{ 'heaterStatus' : '" + status + "' }";
            BrokeredMessage message = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(utf8String)), true);
            message.CorrelationId = correlationId;

            return message;
        } 
    }
}
