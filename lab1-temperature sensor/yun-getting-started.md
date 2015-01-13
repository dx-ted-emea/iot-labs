# How to set up your Arduino Yun

The Arduino is a powerful prototyping board with full Internet connectivity. Here is a digest of links & how-to tips on how to set it up.

## Software

You should first install the Arduino software on your machine. For Windows, the installer includes some device drivers that should be installed before you plug in the board.

Arduino software: http://arduino.cc/en/main/software

You should download the latest beta version (1.5.8 as of this time) that suports the Yun board.

## Power

Your Yun will need power! The easiest option is to use a micro-USB cable to plug your Yun on your computer. This will both power the board and allow you to program the board via USB.

Once you are plugged in via USB, you can test your board with the Blinky sketch and make sure everything is OK. In the Arduino "Tools" drop-down menu, make sure you selected the right board type and COM port before you try uploading a sketch.

## Network

The Yun comes with built-in Ethernet and Wi-Fi connectivity. This quick how-to guide will show you how to use both to give your Yun access to the network.

### Ethernet

This is the easiest option: just plug in an Ethernet cable, and the Yun will get an IP address via DHCP.

### Wi-Fi

When you initially turn on the Yun, it will create a WiFi network called "Arduino Yun"-XXXXXXXXXXXX. Connect to this WiFi network using your computer or a smartphone (there is no password initially).

Once you are connected, browse to http://arduino.local or http://192.168.240.1 using a Web browser.

In the login form, enter "arduino" as the password.

You will now see a big friendly "Configure" button. Press it to connect your Yun to an existing WiFi network.

At the bottom of the form, find the list of wireless networks and select the one you want to connect to. Enter the password if necessary.

Press "Configure & Restart" to finalize your board configuration.

Your board should now restart and get an IP address from the selected WiFi network.

## Finding your network information

Sometimes finding your Yun IP address might be difficult. There is a handy "WiFiStatus" sketch in Examples / Bridge that will print the current IP address of your Yun to the serial monitor.
