using System;

namespace SDCPervomayDesktop.Models
{
    public class Visitor
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime ArrivalTime { get; set; } = DateTime.Now;
        public string AssignedLockerNumber { get; set; }
    }
}
