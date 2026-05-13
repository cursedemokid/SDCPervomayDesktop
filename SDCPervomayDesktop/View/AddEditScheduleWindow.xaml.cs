using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SDCPervomayDesktop;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditScheduleWindow : Window
    {
        private readonly bool _isEditMode;
        private readonly Sections _currentSection;
        private ObservableCollection<ScheduleItems> _scheduleItems;
        private readonly List<ScheduleItems> _pendingRemovals = new List<ScheduleItems>();

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
                _currentSection = null;
                _scheduleItems = new ObservableCollection<ScheduleItems>();
                ScheduleListBox.ItemsSource = _scheduleItems;
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление расписания для секции";
            }
        }

        private void LoadSections()
        {
            Sections = new ObservableCollection<Sections>(
                App.context.Sections.Where(s => s.IsActive).ToList()
            );
            SectionCombo.ItemsSource = Sections;
        }

        private void LoadSchedule()
        {
            _pendingRemovals.Clear();
            _scheduleItems = new ObservableCollection<ScheduleItems>(_currentSection.ScheduleItems.ToList());
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
            var sectionId = selectedSection.Id;

            foreach (var item in _scheduleItems)
            {
                item.SectionId = sectionId;
                App.context.ScheduleItems.Add(item);
            }

            App.context.SaveChanges();
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

            foreach (var rem in _pendingRemovals)
            {
                var tracked = App.context.ScheduleItems.Find(rem.Id);
                if (tracked != null)
                    App.context.ScheduleItems.Remove(tracked);
            }

            _pendingRemovals.Clear();

            foreach (var item in _scheduleItems.Where(i => i.Id == 0))
            {
                item.SectionId = _currentSection.Id;
                App.context.ScheduleItems.Add(item);
            }

            App.context.SaveChanges();
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
                _scheduleItems.Add(dialog.ScheduleItem);
        }

        private void EditScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItems;
            if (item == null) return;

            var dialog = new AddScheduleItemWindow(item);
            if (dialog.ShowDialog() == true)
                ScheduleListBox.Items.Refresh();
        }

        private void DeleteScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItems;
            if (item == null) return;

            if (FeedbackService.Question($"Удалить расписание для {item.DayName}?") != MessageBoxResult.Yes)
                return;

            if (item.Id != 0)
                _pendingRemovals.Add(item);

            _scheduleItems.Remove(item);
        }
    }
}
