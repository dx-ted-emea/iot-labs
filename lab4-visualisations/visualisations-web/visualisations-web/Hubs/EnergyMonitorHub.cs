using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using visualisations_web.DataAccess;

namespace visualisations_web.Hubs
{
    public class EnergyMonitorHub : Hub
    {
        static Dictionary<string, RedisReader<EnergyMonitorHub>> runningTasks = new Dictionary<string, RedisReader<EnergyMonitorHub>>();

        public void StartReadingPump() {
            var deviceId = "DeviceId";
            Groups.Add(Context.ConnectionId, deviceId);
            
            if (!runningTasks.ContainsKey(deviceId))
                runningTasks.Add(deviceId, new RedisReader<EnergyMonitorHub>(deviceId));
        }
    }
}