var wifi = require('wifi-cc3000');
var tessel = require('tessel');
var config = require('./config');

var AzureEventHubManager = require("./AzureEventHubManager.js")
var aehm = new AzureEventHubManager(config.eventhub_namespace, config.eventhub_hubname ,config.eventhub_keyname, config.eventhub_keyvalue)

if (wifi.isConnected())
{
  var led1 = tessel.led[0].output(1);
  var led2 = tessel.led[1].output(0);

  setInterval(function () {

      var payload = { 'deviceid':'Device01','temperature':32.1,'timestamp':'2015-01-07T16:45:00' };
      aehm.sendMessage(JSON.stringify(payload), 'Device01', config.eventhub_sas);

      led1.toggle();
      led2.toggle();
  }, 100);
}
else
{
  console.error('This lab requires a wifi connect. See http://start.tessel.io/wifi')
}
