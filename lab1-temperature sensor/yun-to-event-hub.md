# Connecting the Arduino Yun to Azure Event Hubs

The Yun really packs two different environments on a single board: an ATmega32u4 microcontroller, roughly of the same class as the one used on the Uno, plus an Atheros AR9331 processor running a Linux distribution based on OpenWRT. The Atheros processor is a Wi-Fi SOC typically used in Access Points and router platforms. This dual architecture allows you to keep running your low-level software on the microcontroller, and offload all the networking heavy lifting to the OpenWRT side of the board, where you have full support for pretty much any stack, protocol or utility you can run on the OpenWRT Linux derivative, like cURL, Python, and of course SSL in the form of OpenSSL or other lightweight stacks like PolarSSL.

This means that the Yun has everything on board that you need to securely send data to Azure Event Hubs over SSL, typically invoking a cURL command on the Linux side via the Bridge library.

## The Shared Access Signature

The Shared Access Signature (SAS) is the means by which requests sent to Azure Event Hubs are secured. No request will be allowed that does not include an HTTP `Authorization` header with a correct SAS token.

The SAS token is based on an access policy, that specifies what can be accomplished using a SAS token generated for that policy; typical permissions include Listen, Send and Manage. You will create a policy under your Event Hub management page, in the Configure tab. In the Shared Access Policies section, give the policy a name and select at least Send so that you can send data, then click Save at the bottom of the page. A corresponding Policy Key will be generated. This key will be used to generate (sign) the SAS tokens.

You need to generate a SAS that exactly matches the URL you are using to send data. The easiest way to create it is to use the [Event Hubs SAS Generator](http://eventhubssasgenerator.azurewebsites.net/) Web site. Just enter all the required parameters in the form, and it will generate a SAS key for you:

- Service Namespace: your top-level Service Bus namespace, e.g. the <something> in <something>.servicebus.windows.net
- Service Path: the name of your Event Hub
- Key Name / Policy: the name of the Shared Access Policy you created
- Policy Key: the corresponding Key
- Expiration Date/Time: expiration date in US format, e.g. mm/dd/yyyy

## Sending data using cURL

Azure Event Hubs uses plain old HTTPS, this means that you can send data from the command line using cURL. A typicall command will look like this:

	curl -XPOST -H Authorization:'SharedAccessSignature sr=https%3A%2F%2Ftomhub-ns.servicebus.windows.net%2Ftessel%2Fpublishers%2Fdevice001%2Fmessages&sig=lnO3iPtghm1xO30sZGW%2Fl7fJF7inKm66Y%2BYFI60XhnU%3D&se=1423163836&skn=send' -d'test' https://tomhub-ns.servicebus.windows.net/tessel/publishers/device001/messages

Let's break down the components:

- The -XPOST parameter means this is a POST request
- The -H is used to add HTTP headers to the request. In this case, we add a header named "Authorization" whose value is the Shared Access Signature (SAS) string (see above)
- The -d parameter specifies which data to send; here a simple string, but it could be a JSON representation or any other string value
- Finally, the URL is in the following form:

	https://<namespace>.servicebus.windows.net/<hub>/publishers/<deviceid>/messages

You will find most of the URL components in your Service Bus / Event Hubs configuration:

- `namespace` is your top-level Service Bus name
- `hub` is the name of your Event Hub
- `deviceid` is a unique identifier (a string) for your device that uniquely identifies it.

The URL must match the one that is included in the SAS.

## Using the Bridge library

Now that we know how to send data using cURL, how do we actually do that from an Arduino sketch?

The Bridge library includes a facility named Process, that is designed to let you invoke processes on the Linux side from the code running on the microcontroller. All you need to do is to call cURL using that library, as shown in the code below.

cURL is called with a `-w` parameter that means that the HTTP result code should be written to standard output when the request is done. This code is then read by the sketch and printed to Serial. It should read "201" if everything went well.

You can look at the [full source code](src/yun/yun_event_hub.ino) for a read-to-use sketch.

```c
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
```
