# IoT Smart home Hackathon Lab #

## Lab 3 Device Control ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

This lab relates to the control of devices from Cloud backed remote resources in order to achieve control over environments in a Smart Home.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Architecture ###

Here an overview of the Lab3; BusinessLogic->EventHub->Pi->Arduino

# Building a Controllable Device #

## Components ##

1. A cloud service encapsulated some eventing technology; a state change occurs which requires an external action to occur. For instance, the Cloud Service detects that the temperature in the Smart Home has dropped below a threshold and the heating should be activated.
2. Field gateway; a generic computer, Raspberry Pi or similar. The role of this machine is to interface with resource constrained devices and provide up-lift capacity such as network and security. 
3. A field device, such as a heater which can be remotely activated. In our Lab we will not use mains voltage (if you want to control mains, take great care and use a mains capable relay), but instead simulate this device using a simple LED, the operation of which is identical from the Arduino's point of view. 

## Field Gateway ##

The purpose of the Field Gateway is to lift up resource constrained devices and expose their function over higher requirement systems such as a network. In this example, the Field Gateway will be provided by a Raspberry PI with an Arduino Uno providing the constrained device role. The connectivity to the Arduino couldn't be simpler Electronically; use the USB on the Arduino to connect to the PI.

![](pi-to-arduino.png)

## Controllable Device Contract ##

It is common for a controllable device to communicate in a well understood protocol that is specific for that device. When building our components we are free to implement our own protocols. Typically devices will use numeric codes to confer meaning of operations, as every saving in data transfer relates directly to a saving in overall device performance, power consumption and ultimately cost of components that are capable of fulfilling our requirements. However, given that these codes are a little arcane (0x03 is turn on device!) we will instead send ASCII literals that are human readable. 

Our device, an Arduino Uno, will read from its Serial port in order to receive commands from the field gateway. It will then write to the same Serial port in order to send reply messages, confirming an action has been undertaken, replying to a query or reporting an error. 

The interface will have the following contract:

- SEND: "ON", the device powers on and replies with its state if successful, which is described as "ON"
- SEND: "OFF", the device powers off and replies "OFF"
- SEND: "QUERY", the device replies with "ON" or "OFF"
- SEND &lt;anything else&gt;, the device replies with "ERR: &lt;anything else&gt;"

### Implement Device Contract ###

```c
 boolean poweredOn = false;
 
void setup() {                
  // initialize the digital pin as an output.
  // Pin 13 has an LED connected on most Arduino boards:
  pinMode(13, OUTPUT);      
  Serial.begin(9600);  //Start the serial connection with the computer
                       //to view the result open the serial monitor 
                       
  Serial.setTimeout(10000);
  
  // cycle LED to prove this electrically circuited
  flash(1000);
}

void flash(int duration) {
    // cycle LED to prove this electrically circuited
  digitalWrite(13, HIGH);   // set the LED on
  delay(duration);              // wait for a second
  digitalWrite(13, LOW);    // set the LED off
  delay(duration);  
}


 
void loop() { 
  // wait for a second
  // read the incoming byte:
  
      String content = "";
      char serialdata[80];
      int nchars;
    
      nchars=Serial.readBytesUntil('\0', serialdata, 80);
      content = String(serialdata);
      content = content.substring(0, nchars);
      content.trim();
      
      if (content != "") {  
        if (content == "ON") 
        {
          digitalWrite(13, HIGH);
          poweredOn = true;
            Serial.print("ON");
        }
        else if (content == "OFF")
        {
          digitalWrite(13, LOW);
          poweredOn = false;
            Serial.print("OFF");
        }
        else if (content == "QUERY")
        {
          if (poweredOn) 
          {
            Serial.print("ON");
          }
          else 
          {
            Serial.print("OFF");
          }                  
        }
        else
        {
          Serial.print("ERR: " + content);
        }
      }
}
```

## Build a Field Gateway ##

The field gateway is a device with a higher capability than the output device and takes responsibility for network traffic and security concerns regarding the communication from the Cloud Service to the Smart Home. 

The field gateway can be any capable device from a PC to an advanced piece of custom electronics. In this instance we will be using a Raspberry Pi, which fits between electronics and computers, it is a small, low capability, low cost piece of electronics that runs Linux, a full OS more normally found on PCs. 

Alternatives to the Raspberry Pi are pcDuino, Arduino Yún or a tablet. The base requirements for this part of the Lab are a USB connection, some form of networking (provided by a WIFI dongle on the Raspberry Pi) and the capability to run Python. Many alternatives to the Pi are possible that support these requirements, so this secton of the lab is considered **portable** between multiple Field Gateways.

### Higher Capabilities? ###

The capabilities of the Field Gateway are not particularly enormous. We need the Field Gateway to communicate with the controllable device over Serial (not necessarily over USB as the Arduino provides UART pins; we are using USB for simple wiring) and we need the Field Gateway to be able to communicate over a network in a stardards compliant manner.

The standard we will be using is AMQP; the AMQP bus will be the Azure Service Bus and the AMQP client will be the Field Gateway. The tool we will use on the Field Gateway to achieve AMQP is [Apache Qpid](http://qpid.apache.org/overview.html).

> Apache Qpid™ makes messaging tools that speak AMQP and support many languages and platforms.
> 
> AMQP is an open internet protocol for reliably sending and receiving messages. It makes it possible for everyone to build a diverse, coherent messaging ecosystem.

It is possible for us to use many languages and platform whilst using the Qpid library; since we are using the Raspberry Pi, Python is a natural choice for us. The Qpid library that supports Python is called **Proton**.

Note that we can swap Python for another language and swap the Raspberry Pi for another platform, and provided Qpid (or another project) has AMQP support, our Field Gateway can be on different hardward. 

If you chose to deploy onto a Microsoft Windows based Field Gateway (PC or Tablet) you gain the additional option of using the native Azure ServiceBus SDK, which doesn't use AMQP but a proprietary standard. 

### Implementing a Field Gateway ###

The Field Gateway utilises demjson for serialization, providing a better tolerance for json strings in non-standard delimits and qpid proton for messaging.

It is important to note that the AMQP communication respects CorrelationIds of the original received message and a subsequent reply is annotated with the same CorrelationId so that the cloud service is able to utilise a request-response messaging pattern. 

```python

import sys, optparse, demjson, serial, time
from proton import *

class MessageSender:
  def send(self, sendUri, message, correlation_id, group_id):
    mng = Messenger()
    mng.start()

    msg = Message()
    msg.address = sendUri
    msg.correlation_id = correlation_id
    msg.group_id = group_id
    msg.body = unicode(demjson.encode(message))
    mng.put(msg)

    mng.send()
    print "sent:" + demjson.encode(message)
    mng.stop()
    return

class ArduinoController:
  def __init__(self, portAddress):
    self.port = serial.Serial(portAddress, 9600, 8)

  def query(self):
    self.port.write("QUERY\0")
    time.sleep(0.3)
    reply = self.port.read(self.port.inWaiting())
    return reply

  def on(self):
    self.port.write("ON\0")
    time.sleep(0.3)
    reply = self.port.read(self.port.inWaiting())
    return reply

  def off(self):
    self.port.write("OFF\0")
    time.sleep(0.3)
    reply = self.port.read(self.port.inWaiting())
    return reply    

defaultUri = "amqps://RootManageSharedAccessKey:v2vGRS15kRoeZhb++6M77BS7IplSXCdvMfZnfrwP97M=@iotlabs.servicebus.windows.net/"

parser = optparse.OptionParser(usage="usage: %prog [options]", description="IoT Hackathon Labs Field Gateway")

parser.add_option("-a", "--address", default=defaultUri,
		  help="Address of the Azure Service Bus the field gateway should utilise")

sendTopic = "fieldgatewaytobusinessrules"
receiveTopic = "businessrulestofieldgateway"

opts, args = parser.parse_args()
if not args:
	args = ["Sample message"]

heaterDevice = ArduinoController("/dev/ttyACM0")

receiveEndpoints = [ defaultUri + "/" + receiveTopic + "/Subscriptions/all" ]

mng = Messenger()
mng.incoming_window = 1
mng.start()

for a in receiveEndpoints:
  mng.subscribe(a)

msg = Message()
while True:
  mng.recv()
  while mng.incoming:
    try:
      mng.get(msg)
    except Exception, e:
      print e
    else:
      #print msg.address, msg.subject or "(no subject)", msg.properties, msg.body, msg.correlation_id, msg.id, msg.reply_to
      print msg.body
      msgJson = demjson.decode(msg.body)

      status = "unknown"
      if "action" in msgJson:
        if msgJson["action"] == "on":
          status = heaterDevice.on();
        elif msgJson["action"] == "off": 
          status = heaterDevice.off();
        elif msgJson["action"] == "query":
          status = heaterDevice.query();

        sender = MessageSender()
        sender.send(defaultUri + sendTopic, { "heaterStatus" : status }, msg.correlation_id, msg.reply_to_group_id)
      mng.accept()

mng.stop()


```

## Communicating To the Field Gateway ##

The reason for connecting Arduino to Raspberry Pi was to provide a secure endpoint that is capable of receiving commands originating in a cloud service. The originator cloud service is an Azure Worker Role that:

- Reads from the heater (via Field Gateway) 
- Reads from the temperatureDb to determine the most recent temperature reading
- Determines whether the temperature is too low or too high, and sets the heater status accordingly
- Notifies visualisation aspects (covered in Lab4) 

The Field Gateway receives messages via AMQP and when it replies it uses the correlationId of all incoming messages in an associated responses. This allows the Cloud Service to implement a request-response pattern in its messaging to the Field Gateway.

```csharp
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
```