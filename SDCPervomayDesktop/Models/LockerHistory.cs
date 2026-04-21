using System;

namespace SDCPervomayDesktop.Models
{
    public class LockerHistory
    {
        public DateTime EventTime { get; set; } = DateTime.Now;
        public string LockerNumber { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
    }
}
