var Store = require('nitrogen-memory-store');

var config = {
    host: process.env.HOST_NAME || 'api.nitrogen.io',
    http_port: process.env.PORT || 443,
    protocol: process.env.PROTOCOL || 'https',
    api_key: process.env.API_KEY,
    mqtt_host: process.env.MQTT_HOST_NAME || 'tomlinuxdevbox.cloudapp.net',
    mqtt_port: 1883,
    mqtt_ssl_port: 8883,
    eventhub_namespace : "tomiot",
    eventhub_hubname : "energy",
    eventhub_keyname : "nitrogen",
    eventhub_keyvalue : "AUhrxxxxSPofxxxxWa9oxxxxqFnnxxxx3ZW7xxxxYAI=",
    username : "54940602cbc4724e044f932e",
    password : "<access_token>",
    log_levels: [ "debug", "info", "warn", "error" ]
};

config.store = new Store(config);

module.exports = config;
