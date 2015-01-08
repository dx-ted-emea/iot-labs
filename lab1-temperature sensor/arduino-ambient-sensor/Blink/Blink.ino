/*
  based on Blink, Arduino's "Hello World!"
  Turns on an LED on for one second, then off for one second, repeatedly.
  The Tinkerkit Led Modules (T010110-7) is hooked up on O0
 
 
  This example code is in the public domain.
 */
 
#define O0 11
#define O1 10
#define O2 9
#define O3 6
#define O4 5
#define O5 3
#define I0 A0
#define I1 A1
#define I2 A2
#define I3 A3
#define I4 A4
#define I5 A5
 
 
void setup() {                
  // initialize the digital pin as an output.
  // Pin 13 has an LED connected on most Arduino boards:
  pinMode(O3, OUTPUT);      
  Serial.begin(9600);  //Start the serial connection with the computer
                       //to view the result open the serial monitor 
}
 
void loop() {
  digitalWrite(O3, HIGH);   // set the LED on
  delay(1000);              // wait for a second
  digitalWrite(O3, LOW);    // set the LED off
  delay(1000);              // wait for a second
  
   int reading = analogRead(I0);  
   
   // converting that reading to voltage, for 3.3v arduino use 3.3
   float voltage = reading * 5.0;
   voltage /= 1024.0; 
   
   // print out the voltage
   Serial.print(voltage); Serial.println(" volts");
   
   // now print out the temperature
   float temperatureC = (voltage - 0.5) * 100 ;  //converting from 10 mv per degree wit 500 mV offset
                                               //to degrees ((volatge - 500mV) times 100)
   Serial.print(temperatureC); Serial.println(" degrees C");
   
   // now convert to Fahrenheight
   float temperatureF = (temperatureC * 9.0 / 5.0) + 32.0;
   Serial.print(temperatureF); Serial.println(" degrees F");
  
}
