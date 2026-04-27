using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditScheduleWindow : Window
    {
        private bool _isEditMode;
        private Sections _currentSection;
        private ObservableCollection<ScheduleItems> _scheduleItems;

        public ObservableCollection<Sections> Sections { get; set; }

        public AddEditScheduleWindow(Sections sectionToEdit = null)
        {
            InitializeComponent();
            DataContext = this;

            LoadSections();

            if (sectionToEdit != null)
            {
                _isEditMode = true;
                _currentSection = sectionToEdit;
                SectionCombo.SelectedValue = _currentSection.Id;
                SectionCombo.IsEnabled = false;
                LoadSchedule();
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                Title = $"Редактирование расписания: {_currentSection.Title}";
            }
            else
            {
                _isEditMode = false;
                _currentSection = new Sections();
                _scheduleItems = new ObservableCollection<ScheduleItems>();
                ScheduleListBox.ItemsSource = _scheduleItems;
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление расписания для секции";
            }
        }

        private void LoadSections()
        {
            // Заглушка
            Sections = new ObservableCollection<Sections>
            {
                new Sections { Id = 1, Title = "Футбол" },
                new Sections { Id = 2, Title = "Баскетбол" },
                new Sections { Id = 3, Title = "Плавание" }
            };
            SectionCombo.ItemsSource = Sections;
        }

        private void LoadSchedule()
        {
            _scheduleItems = new ObservableCollection<ScheduleItems>(_currentSection.ScheduleItems);
            ScheduleListBox.ItemsSource = _scheduleItems;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SectionCombo.SelectedItem == null)
            {
                FeedbackService.Error("Выберите секцию.", "Ошибка");
                return;
            }
            if (_scheduleItems.Count == 0)
            {
                FeedbackService.Error("Добавьте хотя бы один день расписания.", "Ошибка");
                return;
            }
            var selectedSection = (Sections)SectionCombo.SelectedItem;
            selectedSection.ScheduleItems = _scheduleItems;
            FeedbackService.Information("Расписание добавлено!", "Успех");
            DialogResult = true;
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_scheduleItems.Count == 0)
            {
                FeedbackService.Error("Добавьте хотя бы один день расписания.", "Ошибка");
                return;
            }
            _currentSection.ScheduleItems = _scheduleItems;
            FeedbackService.Information("Изменения сохранены!", "Успех");
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddDayBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddScheduleItemWindow();
            if (dialog.ShowDialog() == true)
            {
                _scheduleItems.Add(dialog.ScheduleItem);
            }
        }

        private void EditScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItems;
            if (item == null) return;

            var dialog = new AddScheduleItemWindow(item);
            if (dialog.ShowDialog() == true)
            {
                item.DayName = dialog.ScheduleItem.DayName;
                item.StartTime = dialog.ScheduleItem.StartTime;
                item.EndTime = dialog.ScheduleItem.EndTime;
                ScheduleListBox.Items.Refresh();
            }
        }

        private void DeleteScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItems;
            if (item == null) return;

            if (FeedbackService.Question($"Удалить расписание для {item.DayName}?") == MessageBoxResult.Yes)
            {
                _scheduleItems.Remove(item);
            }
        }
    }
}