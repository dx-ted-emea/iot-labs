using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace visualisations_web.Hubs
{
    public class TemperatureHub : Hub
    {
        static Dictionary<string, DbReader<TemperatureHub>> runningTasks = new Dictionary<string, DbReader<TemperatureHub>>();

        public void StartReadingPump()
        {
            var deviceId = "device0";
            Groups.Add(Context.ConnectionId, deviceId);

            if (!runningTasks.ContainsKey(deviceId))
                runningTasks.Add(deviceId, new DbReader<TemperatureHub>(deviceId));
        }
    }
}