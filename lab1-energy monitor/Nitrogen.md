# Cloud Gateway (nitrogen.io) #

The reality of real world devices is that most are often intermittently connected to the network. This might be because they are battery powered, because they are in motion and out of network coverage, or because they rely on another device for network access, but in the end, for a large class of devices, you can’t assume constant connectivity.

Nitrogen is built around messaging to overcome this, enabling devices to send and receive messages when they are connected, and nearly everything in Nitrogen revolves around this concept.

Messaging also provides abstraction. Messages in Nitrogen follow well known schemas, which enables devices and applications to communicate with each other without having to know the details of the other’s implementation.

In this example we will be making use of the MQTT bridge allows lower capability devices that can only send or recieve MQTT messages to participate in the Nitrogen ecosystem.

## Pre-requisites ##

- create an account at [http://nitrogen.io](http://nitrogen.io)
- Create a Linux VM in Azure (Ubuntu)
- Create an endpoint for port 1883 on the created VM

## Create the files ##

- Create a new file `config.js` and set the contents as shown.  We will replace values later in the exercise.

	var Store = require('nitrogen-memory-store');
    
    var config = {
        host: process.env.HOST_NAME || 'api.nitrogen.io',
        http_port: process.env.PORT || 443,
        protocol: process.env.PROTOCOL || 'https',
        api_key: process.env.API_KEY,
        mqtt_host: process.env.MQTT_HOST_NAME || '**<CloudServiceName>**',
        mqtt_port: 1883,
        mqtt_ssl_port: 8883,
        eventhub_namespace : "**<namespace>**",
        eventhub_hubname : "**<hubname>**",
        eventhub_keyname : "**<username>**",
        eventhub_keyvalue : "**<password>**",
        username : "**<ID>**",
        password : "**<ACCESS TOKEN>**",
        log_levels: [ "debug", "info", "warn", "error" ]
    };
    
    config.store = new Store(config);
    
    module.exports = config;

- Create a new file `AzureEventHubManager.js`, this will be used to forward any received messages to a Azure Event Hub.

	var https = require('https');
    var crypto = require('crypto');
    var moment = require('moment')
    
    var namespace = null;
    var hubname = null;
    var keyvalue = null;
    var keyname = null;
    
    
    function AzureEventHubManager(namespace, hubname, keyname, keyvalue)
    {
        this.namespace = namespace;
        this.hubname = hubname;
        this.keyname = keyname;
        this.keyvalue = keyvalue;
    }
    
    AzureEventHubManager.prototype = {
        create_sas_token: function(uri)
        {
            // Token expires in one hour
            var expiry = moment().add(1, 'hours').unix();
    
            var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
            var hmac = crypto.createHmac('sha256', this.keyvalue);
            hmac.update(string_to_sign);
            var signature = hmac.digest('base64');
            var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + this.keyname;
    
            return token;
        },
        getOptions : function(sas, payload, devicename)
        {
            return {
                hostname: this.namespace + '.servicebus.windows.net',
                port: 443,
                path: '/' + this.hubname + '/publishers/' + devicename + '/messages',
                method: 'POST',
                headers: {
                    'Authorization': sas,
                    'Content-Length': payload.length,
                    'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
                }
            };
        },
        sendMessage : function(payload, devicename)
        {
            var uri = 'https://' + this.namespace + '.servicebus.windows.net' + '/' + this.hubname + '/publishers/' + devicename + '/messages';
    
            var sas = this.create_sas_token(uri);
            var options = this.getOptions(sas, payload, devicename);
    
            var req = https.request(options, function(res) {
                console.log("statusCode: ", res.statusCode);
                console.log("headers: ", res.headers);
    
                res.on('data', function(d) {
                    process.stdout.write(d);
                });
            });
    
            req.on('error', function(e) {
                console.error(e);
            });
    
           var res =  req.write(payload);
            req.end();
        }
    
    }
    
    module.exports = AzureEventHubManager

- Create a new file `server.js` and set the contents as shown.  This will intercept any messages published by the client, validate a connection using the credentials provided and forward the message to a Azure Event Hub

	var config = require('./config')
      , mqtt = require('mqtt')
      , nitrogen = require('nitrogen');
    
    //TODO: need to move config to file
    var AzureEventHubManager = require("./AzureEventHubManager.js")
    var aehm = new AzureEventHubManager(config.eventhub_namespace, config.eventhub_hubname ,config.eventhub_keyname, config.eventhub_keyvalue)
    
    var service = new nitrogen.Service(config);
    
    var FAILURE = 1;
    var SUCCESS = 0;
    
    var mqttServer = mqtt.createServer(function(client) {
    
        client.on('connect', function(packet) {
            console.log("CONNECT: " + JSON.stringify(packet));
    
            if (!packet.username || !packet.password) {
                console.log("Error: No username or password.");
                return client.connack({ returnCode: 1 });            
            }
    
            var principal = new nitrogen.Device({
                accessToken: {
                    token: packet.password
                },
                id: packet.username,
                nickname: packet.username
            });
    
            service.resume(principal, function(err, session, principal) {
                if (err || !session) {
                    console.log("Error resuming: " + JSON.stringify(err));
                    return client.connack({ returnCode: FAILURE });
                }
                client.principal = principal;
                client.session = session;
    
                return client.connack({ returnCode: SUCCESS });
            });
        });
    
        client.on('publish', function(packet) {
            if (client.session != null) {
                console.log("PUBLISH: " + JSON.stringify(packet));
                console.log("MESSAGE: " + JSON.stringify(packet.payload));
                var message = JSON.parse(packet.payload);
                aehm.sendMessage(packet.payload, message.deviceId)
            }
        });
    
        client.on('subscribe', function(packet) {
            console.log("SUBSCRIBE: " + JSON.stringify(packet));
    
            var granted = [];
            packet.subscriptions.forEach(function(subscription) {
                granted.push(subscription.qos);
                var filter = JSON.parse(subscription.topic);
                client.session.onMessage(filter, function(message) {
                    console.log('Body: ' + JSON.stringify(message.body));
                    client.publish({
                        topic: subscription.topic,
                        payload: JSON.stringify(message.body)
                    });
                });
            });
    
            client.suback({granted: granted, messageId: packet.messageId});
        });
    
        client.on('pingreq', function(packet) {
            console.log("PINGREQ: " + JSON.stringify(packet));
            client.pingresp();
        });
    
        client.on('disconnect', function(packet) {
            console.log("DISCONNECT: " + JSON.stringify(packet));
            client.session.stop();
        });
        
        client.on('close', function(packet) {
            console.log("CLOSE: " + JSON.stringify(packet));
        });
    
        client.on('error', function(e) {
            console.log("ERROR: " + e);
            client.stream.end();
        });
    }).listen(config.mqtt_port);

- Create a new file `client.js` and set the contents as shown.  This will create a client connection to the server using credentials derived from `config.js` and push these to the server for processing.

    var config = require('./config')
        , mqtt = require('mqtt');
    
    var client = mqtt.createClient(config.mqtt_port, config.mqtt_host, {
        'username': config.username,
        'password': config.password
    });
    
    var baseReading = 34694
    
    client.on('connect', function () {
    
        while(true)
        {
            baseReading += Math.floor( Math.random() * ( 1 + 10 - 0 ) ) + 0
            var now = new Date()
            var obj = {timestamp: now,
                deviceId : "DeviceId",
                reading : baseReading};
    
            var payload = JSON.stringify(obj)
    
            var result = client.publish('messages', payload );
            console.log(result)
        }
    });
    

## Setup Server ##

Now we have the files we need, the server VM should be configured so it can run the node app.

- Connect to the VM via SSH
- Execute the following commands to install nodejs and npm

	sudo apt-get install nodejs
	sudo apt-get install npm

- Install the nitrogen toolset

	sudo npm install -g nitrogen-cli
	sudo ln -s /usr/bin/nodejs  /usr/bin/node

- execute the following command to login to nitrogen (replace <email> as appropriate), enter the password to authenticate.

	n2 principal login **<email>**

- Execute the following command and copy the **KEY** (**API-KEY**)

	n2 apikeys ls

- Execute the following command to create a new principal (user) who happens to be a device.  Copy the ID in the result.

	n2 principal create --type device --name 'energyReader’ –apiKey ‘**<API_KEY>**’

- Execute command, copy the resulting **ACCESS TOKEN**

	n2 principal accesstoken <ID>

- The ID and ACCESS TOKEN are used as credentials when authenticating to the server.  
- Edit config.js
- Modify **username**/**password** with the **ID** and **ACCESS TOKEN** above
- Update parameter **mqtt_host** set that to be the cloud service name for the linux host we created.
- Update the parameters **eventhub_namespace**, **eventhub_hubname**, **eventhub_keyname**, **eventhub_keyvalue** to configure the Event Hub to write to
- Create a directory where we will place the code

	mkdir nitrogen-server
	cd nitrogen-server

- Copy the following files to this directory

	AzureEventHubManager.js
	Config.js
	Server.js

- Using the SSH session, run the following commands to install the node packages

	npm install mqtt
	npm install nitrogen
	npm install nitrogen-memory-store
	npm install moment

- Run the server using the following command

	node server.js

## Setup Client ##

-**Currently this works locally, it will automatically generate messages in an infinite loop**
-**ASSUMES WINDOWS FOR now**
- Install node from [http://nodejs.org/download/](http://nodejs.org/download/)
- Create a new directory 

	mkdir nitrogen-client
	cd nitrogen-server

- Copy the following files to this directory

	client.js
	config.js

- Execute the following commands to install node modules

	npm install mqtt
	npm install nitrogen-memory-store

- Start the client 
-
	node client.js
