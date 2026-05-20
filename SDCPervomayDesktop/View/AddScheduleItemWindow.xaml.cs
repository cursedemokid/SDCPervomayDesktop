using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SDCPervomayDesktop.View
{
    public partial class AddScheduleItemWindow : Window
    {
        private readonly ScheduleItems _editItem;
        private static readonly System.Collections.Generic.Dictionary<string, string> DayMap =
            new System.Collections.Generic.Dictionary<string, string>
            {
                ["Понедельник"] = "Пн",
                ["Вторник"] = "Вт",
                ["Среда"] = "Ср",
                ["Четверг"] = "Чт",
                ["Пятница"] = "Пт",
                ["Суббота"] = "Сб",
                ["Воскресенье"] = "Вс"
            };

        public ScheduleItems ScheduleItem { get; private set; }

        public AddScheduleItemWindow(ScheduleItems editItem = null)
        {
            InitializeComponent();
            _editItem = editItem;
            if (editItem != null)
            {
                var displayDay = DayMap.FirstOrDefault(kv => kv.Value == editItem.DayName).Key ?? editItem.DayName;
                DayCombo.SelectedItem = DayCombo.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == displayDay);
                StartTimeBox.Text = editItem.StartTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
                EndTimeBox.Text = editItem.EndTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            }
        }

        private static bool TryParseTimeSpan(string text, out TimeSpan value)
        {
            text = (text ?? string.Empty).Trim();
            if (TimeSpan.TryParse(text, out value))
                return true;

            var formats = new[] { "HH:mm", "H:mm", "hh\\:mm", "h\\:mm" };
            var today = DateTime.Today;
            foreach (var fmt in formats)
            {
                if (DateTime.TryParseExact(text, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    value = dt.TimeOfDay;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseTimeSpan(StartTimeBox.Text, out TimeSpan start) ||
                !TryParseTimeSpan(EndTimeBox.Text, out TimeSpan end))
            {
                FeedbackService.Error("Неверный формат времени. Используйте ЧЧ:ММ", "Ошибка");
                return;
            }

            if (start >= end)
            {
                FeedbackService.Error("Время начала должно быть меньше времени окончания.", "Ошибка");
                return;
            }

            var dayComboItem = DayCombo.SelectedItem as ComboBoxItem;
            if (dayComboItem == null)
            {
                FeedbackService.Error("Выберите день недели.", "Ошибка");
                return;
            }

            var selectedDisplayDay = dayComboItem.Content.ToString();
            var normalizedDayName = DayMap.TryGetValue(selectedDisplayDay, out var shortDayName)
                ? shortDayName
                : selectedDisplayDay;

            if (_editItem != null)
            {
                _editItem.DayName = normalizedDayName;
                _editItem.StartTime = start;
                _editItem.EndTime = end;
                ScheduleItem = _editItem;
            }
            else
            {
                ScheduleItem = new ScheduleItems
                {
                    DayName = normalizedDayName,
                    StartTime = start,
                    EndTime = end
                };
            }

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
