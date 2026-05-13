namespace SDCPervomayDesktop.Entities
{
    /// <summary>Поля только для отображения в UI (привязки XAML).</summary>
    public partial class ScheduleItems
    {
        public string StartTimeString => StartTime.ToString(@"hh\:mm");

        public string EndTimeString => EndTime.ToString(@"hh\:mm");
    }
}
