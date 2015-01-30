using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using visualisations_web.DataAccess;
using visualisations_web.Models;

namespace visualisations_web.Hubs
{
    public class EnergyMonitorHub : Hub
    {
        static Dictionary<string, RedisReader<EnergyMonitorHub>> runningTasks = new Dictionary<string, RedisReader<EnergyMonitorHub>>();

        public void StartReadingPump() {
            var deviceId = "DeviceId0";
            Groups.Add(Context.ConnectionId, deviceId);
            
            if (!runningTasks.ContainsKey(deviceId))
                runningTasks.Add(deviceId, new RedisReader<EnergyMonitorHub>(deviceId));
        }

        public AverageDeviceEnergyUsageByHour[] GetAverages()
        {
            var deviceId = "DeviceId0";

            var connectionString = ConfigurationManager.ConnectionStrings["EnergyDb"].ConnectionString;
            using (AverageEnergyUsageContext ctx = new AverageEnergyUsageContext(connectionString))
            {
                return ctx.Averages.Where(t => t.DeviceId == deviceId).OrderBy(t => t.HourOfDay).ToArray();
            }
        }

    }
}