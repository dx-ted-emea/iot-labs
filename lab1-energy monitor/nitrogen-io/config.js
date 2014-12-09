var Store = require('nitrogen-memory-store');

var config = {
    host: process.env.HOST_NAME || 'api.nitrogen.io',
    http_port: process.env.PORT || 443,
    protocol: process.env.PROTOCOL || 'https',
    api_key: process.env.API_KEY,
    mqtt_host: process.env.MQTT_HOST_NAME || 'nitrogen-nodejs.cloudapp.net',
    mqtt_port: 1883,
    mqtt_ssl_port: 8883,
    eventhub_namespace : "devicestreaming-ns",
    eventhub_hubname : "iotenergystreaming",
    eventhub_keyname : "manage",
    eventhub_keyvalue : "swdrrfU9K/Bh/kgm88nCbk0Whk8MInzaWgmdLTURM3Y=",
    username : "54870665cbc4724e044f5511",
    password : "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiI1NDg3MDY2NWNiYzQ3MjRlMDQ0ZjU1MTEiLCJpYXQiOjE0MTgxMzUxOTMsImV4cCI6MTQxODIyMTU5M30.agyPgN0A0SQsRzDJFNWukQAVM25Cka2aL5h5SyEY3Q0",
    log_levels: [ "debug", "info", "warn", "error" ]
};

config.store = new Store(config);

module.exports = config;
