using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Influxdb_client.Models
{
    public class MeasureRaw
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)    ]
        public int ID { get; set; }
        public DateTime Time { get; set; }=DateTime.Now;
        public string Measurement { get; set; }=null!;
        public string Field { get; set; }=null!;
        public double Value { get; set; }
    }
}
