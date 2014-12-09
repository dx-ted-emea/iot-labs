# IoT Smart home Hackathon Lab #

## Lab 3 Device Control ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

This lab relates to the control of devices from Cloud backed remote resources in order to achieve control over environments in a Smart Home.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Architecture ###

Here an overview of the Lab3; BusinessLogic->EventHub->Pi->Arduino

# Building a Controllable Device #

## Components ##

1. A cloud service encapsulated some eventing technology; a state change occurs which requires an external action to occur. For instance, the Cloud Service detects that the temperature in the Smart Home has dropped below a threshold and the heating should be activated.
2. Field gateway; a generic computer, Raspberry Pi or similar. The role of this machine is to interface with resource constrained devices and provide up-lift capacity such as network and security. 
3. A field device, such as a heater which can be remotely activated. In our Lab we will not use mains voltage (if you want to control mains, take great care and use a mains capable relay), but instead simulate this device using a simple LED, the operation of which is identical from the Arduino's point of view. 

## Field Gateway ##

The purpose of the Field Gateway is to lift up resource constrained devices and expose their function over higher requirement systems such as a network. In this example, the Field Gateway will be provided by a Raspberry PI with an Arduino Uno providing the constrained device role. The connectivity to the Arduino couldn't be simpler Electronically; use the USB on the Arduino to connect to the PI.

![](pi-to-arduino.png)