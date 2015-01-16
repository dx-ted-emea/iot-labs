var config = require('./config');
var AzureEventHubManager = require("./AzureEventHubManager.js");

var device = process.argv[2] || process.exit(1);

var aehm = new AzureEventHubManager(config.eventhub_namespace, config.eventhub_hubname ,config.eventhub_keyname, config.eventhub_keyvalue);
console.log(aehm.create_sas_token("https://" + config.eventhub_namespace + ".servicebus.windows.net/" + config.eventhub_hubname + "/publishers/" + device + "/messages"));
