using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class SchedulePage : Page
    {
        private DateTime currentMonday;

        public SchedulePage()
        {
            InitializeComponent();
            if (App.CurrentEmployee.RoleId == 3)
                AdminGrid.Visibility = Visibility.Collapsed;

            currentMonday = GetMonday(DateTime.Today);
            UpdateWeekLabel();
            LoadDaySchedule("Пн"); // по умолчанию понедельник
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private void UpdateWeekLabel()
        {
            WeekLabel.Text = $"{currentMonday:dd.MM.yyyy} – {currentMonday.AddDays(6):dd.MM.yyyy}";
        }

        private void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            currentMonday = currentMonday.AddDays(-7);
            UpdateWeekLabel();
            LoadDaySchedule(GetActiveDayName());
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            currentMonday = currentMonday.AddDays(7);
            UpdateWeekLabel();
            LoadDaySchedule(GetActiveDayName());
        }

        private string GetActiveDayName()
        {
            // Определяем, какой день выбран (по жирному шрифту)
            if (MondayTbl.FontWeight == FontWeights.Bold) return "Пн";
            if (TuesdayTbl.FontWeight == FontWeights.Bold) return "Вт";
            if (WednesdayTbl.FontWeight == FontWeights.Bold) return "Ср";
            if (ThursdayTbl.FontWeight == FontWeights.Bold) return "Чт";
            if (FridayTbl.FontWeight == FontWeights.Bold) return "Пт";
            if (SaturdayTbl.FontWeight == FontWeights.Bold) return "Сб";
            return "Вс";
        }

        private void HighlightDay(TextBlock selected)
        {
            foreach (var tb in new[] { MondayTbl, TuesdayTbl, WednesdayTbl, ThursdayTbl, FridayTbl, SaturdayTbl, SundayTbl })
                tb.FontWeight = FontWeights.Normal;
            selected.FontWeight = FontWeights.Bold;
        }

        private void MondayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(MondayTbl); LoadDaySchedule("Пн"); }
        private void TuesdayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(TuesdayTbl); LoadDaySchedule("Вт"); }
        private void WednesdayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(WednesdayTbl); LoadDaySchedule("Ср"); }
        private void ThursdayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(ThursdayTbl); LoadDaySchedule("Чт"); }
        private void FridayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(FridayTbl); LoadDaySchedule("Пт"); }
        private void SaturdayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(SaturdayTbl); LoadDaySchedule("Сб"); }
        private void SundayTbl_MouseDown(object sender, MouseButtonEventArgs e) { HighlightDay(SundayTbl); LoadDaySchedule("Вс"); }

        private void LoadDaySchedule(string dayName)
        {
            // Выбираем нужные занятия на указанный день недели
            var rawItems = App.context.ScheduleItems
                .Where(si => si.DayName == dayName)
                .Select(si => new
                {
                    SectionId = si.SectionId,
                    SectionTitle = si.Sections.Title,
                    TrainerId = si.Sections.TrainerId,
                    StartTime = si.StartTime,
                    EndTime = si.EndTime,
                    ImageData = si.Sections.ImageData
                })
                .ToList();

            // Собираем всех тренеров одним запросом
            var trainerIds = rawItems.Select(i => i.TrainerId).Distinct().ToList();
            var trainers = App.context.Employees
                .Where(e => trainerIds.Contains(e.Id))
                .ToDictionary(
                    e => e.Id,
                    e => e.LastName + " " + e.FirstName + " " + (e.Patronymic ?? "")
                );

            // Формируем финальный список
            var items = rawItems.Select(i => new
            {
                i.SectionId,
                i.SectionTitle,
                CoachFullName = trainers.TryGetValue(i.TrainerId, out var name) ? name : "",
                Time = $"{i.StartTime:hh\\:mm} - {i.EndTime:hh\\:mm}",
                ImageSource = i.ImageData != null ? ByteArrayToImage(i.ImageData) : null
            }).ToList();

            ScheduleListBox.ItemsSource = items;
        }

        private ImageSource ByteArrayToImage(byte[] data)
        {
            if (data == null) return null;
            var image = new System.Windows.Media.Imaging.BitmapImage();
            using (var ms = new System.IO.MemoryStream(data))
            {
                image.BeginInit();
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }
            return image;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditScheduleWindow();
            if (window.ShowDialog() == true)
                LoadDaySchedule(GetActiveDayName());
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListBox.SelectedItem == null)
            {
                FeedbackService.Error("Выберите занятие!");
                return;
            }

            dynamic selected = ScheduleListBox.SelectedItem;
            int sectionId = (int)selected.SectionId;
            var section = App.context.Sections.Find(sectionId);

            if (section != null)
            {
                var window = new AddEditScheduleWindow(section);
                if (window.ShowDialog() == true)
                    LoadDaySchedule(GetActiveDayName());
            }
        }
    }
}