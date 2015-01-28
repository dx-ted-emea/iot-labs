# IoT Smart home Hackathon Lab #

## Lab 1 Environmental Sensor ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Key Challenge 1 - Build a Sensor

The first key challenge we face is to be able to interact and respond to our environment. Easily accessible and low cost consumer electronics can be built and programmed to provide insight into the ambient environment that surrounds us. Once we have the ability to detect temperature, we have reached our entry point to engaging with our environment and we can look to enrich, automate and improve our surroundings. 

### Key Challenge 2 - Basic Scalable Ingestion 

A successful device on consumer sale can be measured in the millions of shipped units. Each device is capable of "calling home", reporting their state to remote services, many times a second. Even with a relatively small number of distributed systems, it becomes a challenge to reliably receive and process the weight of messaging that Machine to Machine (M2M) communication makes possible.

In order to solve this challenge, Cloud Platform members such as the Azure Event Hub can be used to offer message receipt and onward delivery.

Furthermore, simply receiving messages is insufficient to meet the challenges of at scale message processing. We instead need to be able to pass these messages onto a processing

In a basic form, we can use Azure Stream Analytics to provide on the fly aggregations and averages of data points flowing through the system.

# Building an Environment Sensor #

##Components##

The Environment Sensor is made of two main pieces of hardware;

1. External environment sensor; a piece of electronics that senses external environmental factors and emits a signal that represents the state. An example is an input device that alters its voltage based on the temperature to which it is exposed. 
2. Sensor control device; an Arduino, Tessel.io, Netduino or similar; a piece of specialist electronics that can connect to many external sensors, gather their signals and perform an action based upon them. In this example, we will focus on using the Tessel.io to achieve the workload required and presenting an approach that is also device agnostic.

Starting with the Sensor control device, we can explore these components.

### Sensor Control Device ###

The Sensor Control Device has the sole function of interfacing with lower level electronics equipment and bridging the logical gap of capability between the devices, allowing the readings from the single function temperature sensor to be wrapped into a network oriented packet and distributed to cloud based services. 

The Sensor Control Device can be constructed in many forms; popular choices include:

1. [Arduino](http://arduino.cc/) controllers, 
2. [Netduino](http://www.netduino.com/) controllers, which are similar to Arduinos but run Microsofts' .net MicroFramework
3. [Raspberry Pi](http://www.raspberrypi.org/)
4. [Tessel](http://tessel.io) - Tessel is a microcontroller that runs JavaScript. It's Node-compatible and ships with Wifi built in. Use it to easily make physical devices that connect to the web.

In this lab we will be using a Tessel, which allows the software developer to write their software in Javascript via Nodejs. This convergence of the low level electronics programming with higher level, popular programming languages is common in modern hobbyist hardware development as it allows hardware vendors to target large audiences. 

#### Discussion Point

> There are many devices coming to market that are not listed here. Important factors for choosing are form factor; cost per unit; ease of development; cost to manufacture; ease of building custom boards based on identical software and required changes; ease of access to components especially with prefabricated modules (i.e. for Tessel). An interesting discussion is to rank these devices based on these criteria.

As an alternative, we have some instructions on using an *Arduino Yun* device below.

### External Environment Sensor ###

This lab continues on with a basic approach to using the Tessel; using the available modules to determine the temperature using the [**Si7005**](http://www.silabs.com/Support%20Documents/TechnicalDocs/Si7005.pdf) based [Climate module](https://tessel.io/docs/climate) for the Tessel. 

The design of the Tessel is modular and it is a simple way of adding capabilities to the Tessel board for various fields of sensation. The Tessel Climate module retails for around $25 (USD) and allows for the measurement of Temperature and Humidity.

![](tessel-climate-1.png)

#### Discussion Point

>This approach makes electrical engineering possible without needing to solder or build circuits yourself, but if you agree with the author that the joy of IoT is creativity, the expression of creativity is to make rather than consume, explore the lower level [Lab 1.1, Arduino](Lab1.1-Arduino.md) chaining. 

# Writing a Tessel based Environment Sensor #

The Tessel [documents](https://tessel.io/docs/climate) are a great starting point for this part of the Lab. Let's go over those in order to make sure our Tessel and Climate Module are working well.

## Basic Tessel setup ##

Using the Tessel the user can write Javascript in a nodejs compliant manner, using similar approaches to how one might write any nodejs applications, such as the use of `npm` and `require('modulename')`

Once the required modules are pulled into Javascript the Tessel has the ability to utilise the Climate module in a very simple way. 

Firstly address the climate module in order set up the dependencies:

```javascript
var climatelib = require('climate-si7005');

var climate = climatelib.use(tessel.port['A']);
```

Once you have access to this, you can continually poll on methods such as `readTemperature` in order to gain a periodic value for the climate modules received temperature:

```javascript
climate.on('ready', function () {
  console.log('Connected to si7005');

  // Loop forever
  setImmediate(function loop () {
    climate.readTemperature('f', function (err, temp) {
      climate.readHumidity(function (err, humid) {
        console.log('Degrees:', temp.toFixed(4) + 'F', 'Humidity:', humid.toFixed(4) + '%RH');
        setTimeout(loop, 300);
      });
    });
  });
});

climate.on('error', function(err) {
  console.log('error connecting module', err);
});
setInterval(function(){}, 20000);
```

So far we have replicated the base function of the Tessel with the Climate module. What we should do now is use this data for something interesting and invoke a remote endpoint hosted in the Cloud with a payload based on the reading that we have just started to receive.

## Event Hubs and Azure ##

Once we a working with the basic setup, we know that we can read temperatures and humidities. The next step is to pass this data across to the Azure Event Hub so that subsequent activities can be undertaken upon it. 

The first thing you will need to do is to create an Azure Event Hub. The following describes the steps to configure an Event Hub to which we can send messages.

1. Login to the Azure Portal
2. Navigate to Service Bus and create a new instance
3. Under Event Hubs, create a new instance
4. Once complete navigate to Configure tab
5. Create a new Shared Access Policy and set permissions to Manage, Send, Listen
6. Save the updated configuration
7. Copy the Policy Name and Primary Key after the Save has completed. These details will be required to connect to the Event Hub.

We will the require our tessel to be setup for [Wifi](http://start.tessel.io/wifi "WIFI") which is simple. If you are undertaking this hackathon in a location where the wifi is gated by a HTML based username and password portal, you may find it easier to tether your Tessel to a shared WIFI from your phone.

```text
tessel wifi -n [network name] -p [password] -s [security type*]
```

Once you are connected to the WIFI, you can create a Tessel project, a folder that contains:

- `TemperatureSensor.js` which will hold our code
- `AzureEventHubManager.js` which helps us connect to event hubs
- `config.js` which contains connection information

Once we have this code in place, we need to modify config.js to add our connection information. You will need to enter the following parameters:

- `eventhub_namespace`: the namespace of your Service Bus, e.g. **iotlabs-ns**
- `eventhub_hubname`: the name of your Event Hub, e.g. **tessel**
- `eventhub_keyname`: the name of your Shared Access Policy key, e.g. **send**
- `eventhub_keyvalue`: the key corresponding to the Shared Access Policy
- `eventhub_sas`: this last parameter will need to be generated as below.

To generate the `eventhub_sas` parameter, you can run the included [generate_sas_token.js](src/tessel/generate_sas_token.js) script and specify the device identifier that will be sending messages.

```
node generate_sas_token.js Device01
```

This will print out a Shared Access Signature token, that you can then paste in your `config.js` file. This token is specific to the parameters you specified, including the device identifier, and it should look like this:

```
SharedAccessSignature sr=https%3A%2F%2Fiotlabs-ns.servicebus.windows.net%2Ftessel%2Fpublishers%2FDevice01%2Fmessages&sig=xxxx&se=1421420716&skn=send
```

We will then need to install a dependency used by AzureEventHubManager; make sure you are in the same folder as temperaturesensor.js and enter:

```text
npm install moment
```

This will also create a `packages.json` file in the same folder as our runtime. This is especially useful as the presence of a `packages.json` helps the Tessel deployment code to package all relevant files; if we didn't have this file we could have entered a situation where not all the dependencies are packaged correctly.

Now that this has been done, we can edit our `temperatureSensor.js` to add in the functionality for the Event Hub. Please note that when sending messages, we are using the same device identifier that we used before to generate the SAS (`Device01` in this example).

```javascript
var wifi = require('wifi-cc3000');
var climatelib = require('climate-si7005');
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
    console.log('Connected to si7005');

    // Loop forever
    setImmediate(function loop () {
      climate.readTemperature('C', function (err, temp) {
        climate.readHumidity(function (err, humid) {

          var payload = { 'deviceid':'Device01','temperature':temp.toFixed(4),'timestamp':Date.toISOString() };
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

```

### Following on 

Once we have the data on the Azure Event Hub, we can being to process this using Azure Stream Analytics with the eventual goal of having it in a SQL data store for later use. 

This is detailed in [Hot Path Analytics](hotpath.md)

## Using an Arduino Yun ##

As an alternative to using a Tessel device, we are providing below some basic instructions on how to use an Arduino Yun to send data to Azure Event Hubs.

- [Getting started with the Arduino Yun](yun-getting-started.md)
- [Sending data to Event Hubs from the Arduino Yun](yun-getting-started.md)
