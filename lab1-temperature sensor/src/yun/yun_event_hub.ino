/*
** This sample Arduino sketch uploads telemetry data to Azure Event Hubs on an Arduino Yun.
** The Bridge library is used to run cURL to send the data securely using SSL.
**
** Thomas Conté @tomconte
*/

#include <Process.h>

// Your Service Bus endpoint
String server = "tomhub-ns.servicebus.windows.net";
// The name of your Event Hub
String hub_name = "tessel";
// The publisher name, usually a device identifier
String device_name = "device001";
// The SAS token that matches the parameters above
String sas_key = "SharedAccessSignature sr=https%3A%2F%2Ftomhub-ns.servicebus.windows.net%2Ftessel%2Fpublishers%2Fdevice001%2Fmessages&sig=lnO3xxxxhm1xxxxxZGW%2Fl7xxxxinKm66Y%2BYFI60XhnU%3D&se=1423163836&skn=send";

Process proc;

/*
** Send an HTTP POST request to the Azure Event Hubs
*/

void send_request(int value)
{
  Serial.print("sending ");
  Serial.println(value);

  proc.begin("curl");
  
  // Allow insecure SSL
  proc.addParameter("-k");

  // POST
  proc.addParameter("-X");
  proc.addParameter("POST");

  // Content-Type
  proc.addParameter("-H");
  proc.addParameter("Content-Type:application/json");

  // Authorization token
  proc.addParameter("-H");
  proc.addParameter("Authorization:" + sas_key);

  // POST body
  proc.addParameter("-d");
  proc.addParameter("{\"value\":" + String(value) + "}");

  // Write HTTP code to standard out
  proc.addParameter("-w");
  proc.addParameter("%{http_code}");

  // POST URI
  proc.addParameter("https://" + server + "/" + hub_name + "/publishers/" + device_name + "/messages");

  // Run the command synchronously
  proc.run();  

  // Read the response and print to Serial
  while (proc.available() > 0) {
    char c = proc.read();
    Serial.print(c);
  }
  Serial.println();
}

/*
** Arduino Setup
*/

void setup()
{
  Bridge.begin();
  Serial.begin(9600); 
  
  while (!Serial){
  ; // wait for Serial port to open.
  }
  
  Serial.println("ready.\n");
}

/*
** Arduino Loop
*/

void loop()
{
  //int val = analogRead(A0);
  int val = 42;
  
  send_request(val);
 
  delay(1000);
}
