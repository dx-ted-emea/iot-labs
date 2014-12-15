using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using businessrules.DataAccess;
using System.ComponentModel.DataAnnotations.Schema;

namespace businessrules.DataAccess
{
    public class TemperatureReadingContext : DbContext
    {
        public TemperatureReadingContext(string connectionString)
            : base(connectionString)
        {

        }

        public DbSet<TemperatureReading> Readings { get; set; }
    }
}