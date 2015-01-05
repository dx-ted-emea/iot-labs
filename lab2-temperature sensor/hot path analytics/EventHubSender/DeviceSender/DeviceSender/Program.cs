using System;
using DeviceSender;

namespace Microsoft.IoTLabs.DeviceSender
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 4)
            {
                Console.WriteLine("usage: DeviceSender sb-namespace event-hub-name sas-policy sas-key");
                return;
            }

            var details = new ConnectionDetails(args[0], args[1], args[2], args[3]);
            var deviceDetails = new DeviceSendingDetails()
            {
                FailureConditions = new[]
                {
                    new FailedDeviceSettings(3, 0.1F),
                    new FailedDeviceSettings(6, 0.2F),
                    new FailedDeviceSettings(9, 0.3F),
                    new FailedDeviceSettings(12, 0.05F),
                    new FailedDeviceSettings(15, 0.07F),
                    new FailedDeviceSettings(18, 0.15F),
                    new FailedDeviceSettings(21, 0.25F)
                },
                IterationSeconds = 30,
                NumberOfDevices = 50,
                TemperatureMax = 28.9F,
                TemperatureMin = 19.6F,
                MillisecondDelay = 1000
            };

            var generator = new MessageGenerator(deviceDetails);
            generator.SendMessages(details);
            Console.WriteLine("Finished sending all messages. Press any key to exit ...");
            Console.Read();
        }
    }
}
