using System;

namespace api.Models
{
    public class RfidEvent
    {
        public int Id { get; set; }
        public int Rfid { get; set; }
        public string EventType { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }
    }
}