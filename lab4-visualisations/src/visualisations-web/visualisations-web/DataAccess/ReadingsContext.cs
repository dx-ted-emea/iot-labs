using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using visualisations_web.Models;

namespace visualisations_web.DataAccess
{
    public class ReadingsContext : DbContext
    {
        public ReadingsContext(string connectionString) : base(connectionString)
        {
            
        }

        public DbSet<Readings> Readings { get; set; }
    }
}