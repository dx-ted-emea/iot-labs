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



