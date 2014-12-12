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
