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
        var endReading = baseReading + Math.floor( Math.random() * ( 1 + 100 - 0 ) ) + 0
        var energyUsage = endReading - baseReading;

        var deviceID = Math.floor( Math.random() * ( 1 + 3 - 0 ) ) + 0
        var now = new Date()
        var obj = {timestamp: now,
            deviceId : "DeviceId" + deviceID,
            startReading : baseReading,
            endReading : endReading,
            energyUsage : energyUsage};

        var payload = JSON.stringify(obj)

        var result = client.publish('messages', payload );
        console.log(payload)

        baseReading = endReading;
    }
});



