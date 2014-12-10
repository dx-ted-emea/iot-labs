# IoT Smart home Hackathon Lab #

## Lab 2 Environmental Sensor ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Architecture ###

Here an overview of the Lab2; (Arduino->)Tessel->EventHub->BusinessLogic

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

### External Environment Sensor ###

This lab continues on with a basic approach to using the Tessel; using the available modules to determine the temperature using the [**Si7005**](http://www.silabs.com/Support%20Documents/TechnicalDocs/Si7005.pdf) based [Climate module](https://tessel.io/docs/climate) for the Tessel. 

The design of the Tessel is modular and it is a simple way of adding capabilities to the Tessel board for various fields of sensation. The Tessel Climate module retails for around $25 (USD) and allows for the measurement of Temperature and Humidity.

![](tessel-climate-1.png)

This approach makes electrical engineering possible without needing to solder or build circuits yourself, but if you agree with the author that the joy of IoT is creativity, the expression of creativity is to make rather than consume, explore the lower level [Lab 2.1, Arduino](Lab2.1-Arduino.md) chaining. 

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

[question; detail here or refer back to earlier lab1 example?]

[Here detail how to use Event Hub with nodejs/tessel] 

