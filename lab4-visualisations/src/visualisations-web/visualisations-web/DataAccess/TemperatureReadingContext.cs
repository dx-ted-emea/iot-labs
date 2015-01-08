using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using visualisations_web.Models;

namespace visualisations_web.DataAccess
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