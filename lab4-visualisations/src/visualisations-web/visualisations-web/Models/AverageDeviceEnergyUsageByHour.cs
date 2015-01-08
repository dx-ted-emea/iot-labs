using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace visualisations_web.Models
{
    [Table("averagesPerHour")]
    public class AverageDeviceEnergyUsageByHour
    {
        [Column("deviceId")]
        public string DeviceId { get; set; }

        [Column("hourOfDay")]
        public int HourOfDay { get; set; }

        [Column("average")]
        public int Average { get; set; }
    }
}