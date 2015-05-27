var wifi = require('wifi-cc3000');
var climatelib = require('climate-si7020');
var tessel = require('tessel');
var config = require('./config');

var climate = climatelib.use(tessel.port['A']);

var AzureEventHubManager = require("./AzureEventHubManager.js")
var aehm = new AzureEventHubManager(config.eventhub_namespace, config.eventhub_hubname ,config.eventhub_keyname, config.eventhub_keyvalue)

if (wifi.isConnected())
{
  var led1 = tessel.led[0].output(1);
  var led2 = tessel.led[1].output(0);

  climate.on('ready', function () {
    console.log('Connected to si7020');

    // Loop forever
    setImmediate(function loop () {
      climate.readTemperature('C', function (err, temp) {
        climate.readHumidity(function (err, humid) {

          var d = new Date();
          var payload = { 'deviceid':'Device01','temperature':temp.toFixed(4),'timestamp':d.toISOString() };
          aehm.sendMessage(JSON.stringify(payload), 'Device01', config.eventhub_sas);

          led1.toggle();
          led2.toggle();

          setTimeout(loop, 300);
        });
      });
    });
  });

  climate.on('error', function(err) {
    console.log('error connecting module', err);
  });

}
else
{
  console.error('This lab requires a wifi connect. See http://start.tessel.io/wifi')
}
