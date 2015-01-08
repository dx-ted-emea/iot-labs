using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace visualisations_web.Models
{
    [Table("avgdevicereadings")]
    public class TemperatureReading
    {
        [Column("readingId"), Key]
        public int ReadingId { get; set; }
        [Column("starttime")]
        public DateTime StartTime { get; set; }
        [Column("endtime")]
        public DateTime EndTime { get; set; }
        [Column("deviceid")]
        public string DeviceId { get; set; }
        [Column("temperature")]
        public double Temperature { get; set; }
        [Column("eventcount")]
        public int EventCount { get; set; }
    }
}
