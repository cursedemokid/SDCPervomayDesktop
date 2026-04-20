using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SDCPervomayDesktop.View
{
    public partial class AddScheduleItemWindow : Window
    {
        public ScheduleItem ScheduleItem { get; private set; }

        public AddScheduleItemWindow(ScheduleItem editItem = null)
        {
            InitializeComponent();
            if (editItem != null)
            {
                // Заполнить поля
                DayCombo.SelectedItem = DayCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == editItem.DayName);
                StartTimeBox.Text = editItem.StartTime.ToString(@"hh\:mm");
                EndTimeBox.Text = editItem.EndTime.ToString(@"hh\:mm");
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!TimeSpan.TryParse(StartTimeBox.Text, out TimeSpan start) ||
                !TimeSpan.TryParse(EndTimeBox.Text, out TimeSpan end))
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

            ScheduleItem = new ScheduleItem
            {
                DayName = dayComboItem.Content.ToString(),
                StartTime = start,
                EndTime = end
            };
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