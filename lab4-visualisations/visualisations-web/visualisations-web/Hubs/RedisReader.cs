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
            var energyCtx = new CurrentEnergyContext("stormredis.redis.cache.windows.net,ssl=true,password=ew8x0rcmtZYhSYNPqPTwnTroSJTMSuHhzyv/X5iQsXs=");
            
            while (true)
            {
                var readings = energyCtx.GetReadings("Device01");
                _hubContext.Clients.Group(deviceId).pump(readings);

                await Task.Delay(1000);
            }
        }
    }
}
