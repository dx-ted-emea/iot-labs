 
int incomingByte = 0;   // for incoming serial data
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
