using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace businessrules.DataAccess
{
    [Table("avgdevicereadings")]
    public class TemperatureReading
    {
        [Column("starttime")]
        public DateTime StartTime { get; set; }
        [Column("endtime")]
        public DateTime EndTime { get; set; }
        [Column("deviceid"), Key]
        public string DeviceId { get; set; }
        [Column("temperature")]
        public double Temperature { get; set; }
        [Column("eventcount")]
        public int EventCount { get; set; }
    }
}
