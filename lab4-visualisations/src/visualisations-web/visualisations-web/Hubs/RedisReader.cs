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
    public class RedisReader<THubContext> where THubContext : Hub
    {
        private IHubContext _hubContext;
        public RedisReader(string deviceId)
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<THubContext>();
            Task.Run(() => Run(deviceId));
        }

        private async Task Run(string deviceId)
        {
            var energyRedis = ConfigurationManager.ConnectionStrings["EnergyRedis"].ConnectionString;
            var energyCtx = new CurrentEnergyContext(energyRedis);
            
            while (true)
            {
                var readings = energyCtx.GetReadings(deviceId);
                _hubContext.Clients.Group(deviceId).pump(readings);

                await Task.Delay(1000);
            }
        }
    }
}
