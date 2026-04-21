using System;

namespace SDCPervomayDesktop.Models
{
    public class Reminder
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueTime { get; set; }
        public bool IsCompleted { get; set; }
        public string Category { get; set; }
    }
}
