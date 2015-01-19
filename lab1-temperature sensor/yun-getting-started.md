# How to set up your Arduino Yun

The Arduino is a powerful prototyping board with full Internet connectivity. Here is a digest of links & how-to tips on how to set it up.

## Software

You should first install the Arduino software on your machine. For Windows, the installer includes some device drivers that should be installed before you plug in the board.

Arduino software: http://arduino.cc/en/main/software

You should download the **latest beta version** (1.5.8 as of this time) that suports the Yun board.

Previous versions do not have support for the Yun. If you previously installed another specialized version (like the one for the Intel Galileo), it won't work with your Yun either.

## Power

Your Yun will need power! The easiest option is to use a micro-USB cable to plug your Yun on your computer. This will both power the board and allow you to program the board via USB.

Once you are plugged in via USB, you can test your board with the Blinky sketch and make sure everything is OK. In the Arduino "Tools" drop-down menu, make sure you selected the right board type and COM port before you try uploading a sketch.

If your Yun does not show up in the Arduino IDE, it might be due to a faulty USB cable. You could also try different USB ports on your machine.

## Network

The Yun comes with built-in Ethernet and Wi-Fi connectivity. This quick how-to guide will show you how to use both to give your Yun access to the network.

### Ethernet

This is the easiest option: just plug in an Ethernet cable, and the Yun will get an IP address via DHCP.

### Wi-Fi

When you initially turn on the Yun, it will create a WiFi network called "Arduino Yun"-XXXXXXXXXXXX. Connect to this WiFi network using your computer or a smartphone (there is no password initially).

**If there are many Yun boards in the same location**, you can't really guess which one is yours! You should run the "WiFi Status" sketch as instructed below to find out your MAC address. The SSID that your Yun broadcasts is based on the MAC address, so this is how you can find out which one is yours!

Once you are connected, browse to http://arduino.local or http://192.168.240.1 using a Web browser.

In the login form, enter "arduino", which is the default password.

You will now see a big friendly "Configure" button. Press it to connect your Yun to an existing WiFi network.

At the bottom of the form, find the list of wireless networks and select the one you want to connect to. Enter the password if necessary.

Press "Configure & Restart" to finalize your board configuration.

Your board should now restart and get an IP address from the selected WiFi network.

# Finding your network information

Sometimes finding your Yun IP address or MAC address might be difficult. There is a handy "WiFiStatus" sketch in Examples / Bridge that will print the current IP address of your Yun to the serial monitor. Run it and you will find all the network information for your board, displayed in the Serial Console:

```
Current WiFi configuration
SSID: Arduino Yun-90A2DAF049BD
Mode: Master
Signal: 100%
Encryption method: None
Interface name: wlan0
Active for: 7 minutes
IP address: 192.168.240.1/255.255.255.0
MAC address: 90:A2:DA:F0:49:BD
RX/TX: 54/164 KBs
```

In this example, the Yun will originally broadcast using the SSID "Arduino Yun-90A2DAF049BD" since this is the board's MAC address.

# Log in to the Web interface

Once your Yun is online, you can connect to its IP address to access its Web management interface.

You can also choose to program and remotely debug your Yun via its IP address instead of the USB cable. This way you can plug the Yun on its own independant power adapter and program it remotely, even if it's in a different room!

# Resetting network settings

If you ever get a bit lost and what to start from scratch again, just hold the **WLAN RST** button for at least 5 seconds, but less than 30 seconds. This will reset the WiFi configuration to the default.

If you want to reset your board completely and restore it to factory settings, hold the WLAN RST button for more than 30 seconds.
