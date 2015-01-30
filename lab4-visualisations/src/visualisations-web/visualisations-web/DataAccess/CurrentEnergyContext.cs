using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using visualisations_web.Models;

namespace visualisations_web.DataAccess
{
    public class CurrentEnergyContext
    {
        private ConnectionMultiplexer _connection;
        public CurrentEnergyContext(string connectionString)
        {
            try
            {
                _connection = ConnectionMultiplexer.Connect(connectionString);
            }
            catch (Exception e)
            { 
            }
        }

        public EnergyMonitorReading[] GetReadings(string deviceId)
        {
            var database = _connection.GetDatabase();

			DateTime dt = DateTime.Now;
			String prefix = String.Format("{0:yyyyMMdd}", dt);
			var values = database.ListRange(prefix + deviceId, -10, -1); // TODO: do not hard code prefix

            var readings = values.Select(t => JObject.Parse(t.ToString()).ToObject<EnergyMonitorReading>()).ToArray();

            var groups = readings.GroupBy(r => r.timestamp.Hour).Select(t => t.Average(x => x.endReading));

            return readings;
        }
    }
}