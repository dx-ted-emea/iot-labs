using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace visualisations_web.Hubs
{
    public class EnergyMonitorHub : Hub
    {
        static Dictionary<string, DbReader<EnergyMonitorHub>> runningTasks = new Dictionary<string, DbReader<EnergyMonitorHub>>();

        public void StartReadingPump() {
            var deviceId = "DeviceId";
            Groups.Add(Context.ConnectionId, deviceId);

            if (!runningTasks.ContainsKey(deviceId))
                runningTasks.Add(deviceId, new DbReader<EnergyMonitorHub>(deviceId));
        }
    }
}