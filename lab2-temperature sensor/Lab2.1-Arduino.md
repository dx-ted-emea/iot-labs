# Environment Sensors #


### Alternative External Environment Sensor approaches ###

In order to detect temperature in an environment, there are many options electronically available to you. You can use a thermometer and rig up some computer vision, a thermister (temperature sensitive resister) or bimetallic strips. To get a more accurate sensor output it is now common to use solid-state technology; as heat is applied to a diode the voltage across it increases in a known way. Typically this around 10mV per degree difference in ambient temperature. This approach is known as a ["silicon bandgap" sensor ](http://en.wikipedia.org/wiki/Silicon_bandgap_temperature_sensor)

Not only is this sensor very accurate compared to the other techniques, it is durable to conditions from -40 deg C to + 150 deg C, it never fatigues as it has no moving parts, it doesn't require calibration and costs a couple of dollars. 

This lab will use a LM35 sensor as its source which we can link to any Sensor control device in a simple way. We can either use a simple direct connection to the LM35 on a breadboard to a sensor control device such as a  Tessel or we can chain through an Arduino which would allow us to communicate with many other sensors. In the chaining example, the Tessel acts very much as a field gateway, in an approach that is further explored in Lab 3.