using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace messagequque.Models
{
    public class TelemetryData
    {
        public long Id { get; set; }

        public string Topic { get; set; } = "";

        public string Payload { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public bool Sent { get; set; }
    }
}
