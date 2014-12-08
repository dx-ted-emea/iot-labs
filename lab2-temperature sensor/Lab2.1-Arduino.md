# Environment Sensors #


## Alternative External Environment Sensor approaches ##

In reality, the Tessel's approach to modularity of sensor extension is optimised for beginners who are more interested in the utility of hardware than the building of the hardware itself. Indeed, the slogan for Tessel is

> Hardware development for software developers. 

Somewhat predicatably this means that the Tessel designers have had to make some compromises with regard to what is possible on the Tessel board. The same is the case for any generic implementation; in order to support a simple generic use case, specific use cases are edged out and become hard to achieve. 

One particular aspect where a little electronic engineering brings big advantages is in repeatability, in terms of cost and fan out. A Tessel can have at most two climate modules connected at any time due to port capacity. The climate modules are also small and physically connected to the Tessel, so having multiple modules connected of the same time is of little benefit in this use case. If you wanted to achieve a smart home that sensed the environment in each room you would need to repeat the $75 Tessel and $25 Climate module for each room, and have each device connect to your cloud service. The tenancy model in the remote cloud service would need to be engineered to a higher extent so that each device could be identified as uniquely owned by the owner. 

We as Electronic Engineers can build a much more cost effective solution by using lower level electronics and chaining them together. 

### Temperature sensing, revisited ###

Beyond the ready made Tessel module, in order to detect temperature in an environment, there are many options electronically available to you. You can use a thermometer and rig up some computer vision, a thermister (temperature sensitive resister) or bimetallic strips. To get a more accurate sensor output it is now common to use solid-state technology; as heat is applied to a diode the voltage across it increases in a known way. Typically this around 10mV per degree difference in ambient temperature. This approach is known as a ["silicon bandgap" sensor ](http://en.wikipedia.org/wiki/Silicon_bandgap_temperature_sensor)

Not only is this sensor very accurate compared to the other techniques, it is durable to conditions from -40 deg C to + 150 deg C, it never fatigues as it has no moving parts, it doesn't require calibration and costs a couple of dollars. 

This lab will use a LM35 sensor as its source which we can link to any Sensor control device in a simple way. We can either use a simple direct connection to the LM35 on a breadboard to a sensor control device such as a  Tessel or we can chain through an Arduino which would allow us to communicate with many other sensors. In the chaining example, the Tessel acts very much as a field gateway, in an approach that is further explored in Lab 3.

### Chaining ### 

Each Tessel has 6 [GPIO](http://en.wikipedia.org/wiki/General-purpose_input/output) ports whereas a single Arduino Uno has 20 of which 14 are Digital and 6 are Analog. Both Tessel and Arduino support [I<sup>2</sup>C](http://en.wikipedia.org/wiki/I%C2%B2C) and this is a perfect use for this technology. The idea is to set the Tessel as an I<sup>2</sup>C Master and have many Arduino I<sup>2</sup>C Slaves. Each of these slaves can be in control of up to 20 other devices, and the Tessel can still be used to provide the Cloud Service connectivity and higher level functions.