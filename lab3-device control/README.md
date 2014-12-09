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

## Controllable Device Contract ##

It is common for a controllable device to communicate in a well understood protocol that is specific for that device. When building our components we are free to implement our own protocols. Typically devices will use numeric codes to confer meaning of operations, as every saving in data transfer relates directly to a saving in overall device performance, power consumption and ultimately cost of components that are capable of fulfilling our requirements. However, given that these codes are a little arcane (0x03 is turn on device!) we will instead send ASCII literals that are human readable. 

Our device, an Arduino Uno, will read from its Serial port in order to receive commands from the field gateway. It will then write to the same Serial port in order to send reply messages, confirming an action has been undertaken, replying to a query or reporting an error. 

The interface will have the following contract:

- SEND: "ON", the device powers on and replies with its state if successful, which is described as "ON"
- SEND: "OFF", the device powers off and replies "OFF"
- SEND: "QUERY", the device replies with "ON" or "OFF"
- SEND &lt;anything else&gt;, the device replies with "ERR: &lt;anything else&gt;"

### Implement Device Contract ###

```c
 boolean poweredOn = false;
 
void setup() {                
  // initialize the digital pin as an output.
  // Pin 13 has an LED connected on most Arduino boards:
  pinMode(13, OUTPUT);      
  Serial.begin(9600);  //Start the serial connection with the computer
                       //to view the result open the serial monitor 
                       
  Serial.setTimeout(10000);
  
  // cycle LED to prove this electrically circuited
  flash(1000);
}

void flash(int duration) {
    // cycle LED to prove this electrically circuited
  digitalWrite(13, HIGH);   // set the LED on
  delay(duration);              // wait for a second
  digitalWrite(13, LOW);    // set the LED off
  delay(duration);  
}


 
void loop() { 
  // wait for a second
  // read the incoming byte:
  
      String content = "";
      char serialdata[80];
      int nchars;
    
      nchars=Serial.readBytesUntil('\0', serialdata, 80);
      content = String(serialdata);
      content = content.substring(0, nchars);
      content.trim();
      
      if (content != "") {  
        if (content == "ON") 
        {
          digitalWrite(13, HIGH);
          poweredOn = true;
            Serial.print("ON");
        }
        else if (content == "OFF")
        {
          digitalWrite(13, LOW);
          poweredOn = false;
            Serial.print("OFF");
        }
        else if (content == "QUERY")
        {
          if (poweredOn) 
          {
            Serial.print("ON");
          }
          else 
          {
            Serial.print("OFF");
          }                  
        }
        else
        {
          Serial.print("ERR: " + content);
        }
      }
}
```