using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Text;
using businessrules.DataAccess;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using businessrules.DeviceControl;

namespace businessrules
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);        

        public override void Run()
        {
            Trace.TraceInformation("businessrules is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("businessrules has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("businessrules is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("businessrules has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var temperatureDbConnectionString = CloudConfigurationManager.GetSetting("TemperatureDbConnectionString");
            var notificationEndpoint = CloudConfigurationManager.GetSetting("NotificationUri");
            var heater = new HeaterCommunication();

            while (true)
            {
                var correlationId = Guid.NewGuid().ToString("n");

                var heaterStatus = heater.Query(correlationId);
                if (heaterStatus == HeaterStatus.UNKNOWN)
                {
                    continue;
                }
                using (TemperatureReadingContext temperatureDb = new TemperatureReadingContext(temperatureDbConnectionString))
                {
                    //get the most recent entry
                    var tempReading = temperatureDb.Readings.OrderByDescending(t=>t.StartTime).First();

                    if (tempReading.Temperature >= 22)
                    {
                        if (heaterStatus == HeaterStatus.ON)
                        {
                            heater.TurnOff(correlationId);
                            NotifyWebUi(notificationEndpoint, false);
                        }
                    }
                    else if (tempReading.Temperature <= 20)
                    {
                        if (heaterStatus == HeaterStatus.OFF)
                        {
                            heater.TurnOn(correlationId);
                            NotifyWebUi(notificationEndpoint, true);
                        }
                    }
                }
                await Task.Delay(5000);//temperature is recorded at a freshness hertz of 60 seconds, check twice as frequently 
            }
        }        

        
        private async Task NotifyWebUi(string notificationEndpoint, bool isTurnedOn)
        {
            var jsonString = JObject.FromObject(new { EventTime = DateTime.Now, IsTurnedOn = isTurnedOn }).ToString();
            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                await client.PostAsync(notificationEndpoint, content);
            }
        }
    }
}
