using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace messagequque.Data
{
    public class MessageEntity
    {
        public long Id { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "pending";
        public int RetryCount { get; set; } = 0;
    }
}
