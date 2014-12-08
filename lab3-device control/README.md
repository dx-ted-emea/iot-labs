# IoT Smart home Hackathon Lab #

## Lab 3 Device Control ##

A Smart Home revolves around the idea that unattended sensors can report on ambient conditions in a home in the homeowners absence. A typical requirement for this is to measure the home environment and to react to this and work to alter the environment to the homeowners wishes. For instance, if the temperature drops below a threshold then a heating system may be activated.

This lab relates to the control of devices from Cloud backed remote resources in order to achieve control over environments in a Smart Home.

In order to implement this, it is possible to take consumer equipment and combine it in a good approximation of how commercial equipment may work. For instance, it is possible to use the **Arduino** consumer electronics boards and standardised components to measure temperature 

### Architecture ###

Here an overview of the Lab2; Arduino->PI->EventHub->BusinessLogic

# Building an Energy Sensor #

##Components##

Sensor control device; an Arduino, Tessel.io, Netduino or similar; a piece of specialist electronics that can connect to many external sensors, gather their signals and perform an action based upon them. Typically these control devices are resource constrained devices that trade capability for customisability and affordability; they may not be network capable but they tend to be able to interface with many sensors at a low level and tend to retail for the price of a trip to the cinema. 
3. Field gateway; a generic computer, Raspberry Pi or similar. The role of this machine is to interface with resource constrained devices and provide up-lift capacity such as network and security. 