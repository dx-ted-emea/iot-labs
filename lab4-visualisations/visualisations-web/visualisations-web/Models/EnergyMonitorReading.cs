using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace visualisations_web.Models
{
    public class EnergyMonitorReading
    {
        //{"timestamp":"2014-12-11T15:00:26.837Z","deviceId":"DeviceId","startReading":15046551,"endReading":15047589,"energyUsage":1038, "serverTimestamp":"2014-12-11T15:00:26.837Z"}
        public DateTime timestamp { get; set; }
        public DateTime serverTimestamp { get; set; }
        public string deviceId { get; set; }
        public int startReading { get; set; }
        public int endReading { get; set; }
        public int energyUsage { get; set; }
    }
}