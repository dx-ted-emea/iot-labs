using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using visualisations_web.Models;

namespace visualisations_web.DataAccess
{
    public class AverageEnergyUsageContext : DbContext
    {
        public AverageEnergyUsageContext(string connectionString)
            : base(connectionString)
        {
            
        }

        public DbSet<AverageDeviceEnergyUsageByHour> Averages { get; set; }
    }
}