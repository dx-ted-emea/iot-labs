using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace visualisations_web.Models
{
    public class Readings
    {
        [Key]
        public int ReadingId { get; set; }
        public int Reading { get; set; }
        public string DeviceId { get; set; }
        public DateTime ServerTimestamp { get; set; }
    }
}
