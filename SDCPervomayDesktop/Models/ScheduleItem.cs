using System;

namespace SDCPervomayDesktop.Models
{
    public class ScheduleItem
    {
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string StartTimeString => StartTime.ToString(@"hh\:mm");
        public string EndTimeString => EndTime.ToString(@"hh\:mm");
    }
}