using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using visualisations_web.DataAccess;

namespace visualisations_web.Hubs
{
    public class DbReader<THubContext> where THubContext : Hub
    {        
        private IHubContext _hubContext;
        public DbReader(string deviceId)
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<THubContext>();
            Task.Run(() => Run(deviceId));
        }

        private async Task Run(string deviceId)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["EnergyDb"].ConnectionString;

            while (true)
            {
                using (ReadingsContext context = new ReadingsContext(connectionString))
                {
                    var readings = context.Readings.Where(t=>t.DeviceId==deviceId)
                        .OrderByDescending(t => t.ServerTimestamp).Take(100).ToList();

                    readings.Reverse();

                    _hubContext.Clients.Group(deviceId).pump(readings);
                }

                await Task.Delay(1000);
            }
        }
    }
}
