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
