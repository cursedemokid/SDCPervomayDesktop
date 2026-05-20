using SDCPervomayDesktop;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class SectionPage : Page
    {
        private List<Sections> allSections;
        private object _selectedListItem;

        public SectionPage()
        {
            InitializeComponent();
            LoadSections();
            if (App.CurrentEmployee.RoleId == 3)
                AdminGrid.Visibility = Visibility.Collapsed;

            SectionListView.SelectionChanged += (s, e) =>
            {
                if (SectionListView.SelectedItem != null)
                    _selectedListItem = SectionListView.SelectedItem;
            };

            SearchTextBox.TextChanged += (s, e) => ApplyFilter();
        }

        private void LoadSections()
        {
            allSections = App.context.Sections.Where(s => s.IsActive).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string search = SearchTextBox.Text?.Trim().ToLower();
            var filtered = allSections;
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(s => s.Title.ToLower().Contains(search)).ToList();

            var trainerIds = filtered.Select(s => s.TrainerId).Distinct().ToList();
            var trainers = App.context.Employees
                .Where(e => trainerIds.Contains(e.Id))
                .ToDictionary(
                    e => e.Id,
                    e => (e.LastName + " " + e.FirstName + " " + (e.Patronymic ?? "")).Trim());

            var sectionIds = filtered.Select(s => s.Id).ToList();
            var daysPerSection = App.context.ScheduleItems
                .Where(si => sectionIds.Contains(si.SectionId))
                .GroupBy(si => si.SectionId)
                .ToDictionary(g => g.Key, g => g.Count());

            SectionListView.ItemsSource = filtered.Select(s => new
            {
                s.Title,
                s.Description,
                TrainerName = trainers.TryGetValue(s.TrainerId, out var trainerName) ? trainerName : "",
                DaysInWeek = daysPerSection.TryGetValue(s.Id, out var days) ? days : 0,
                s.Id
            }).ToList();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditSectionWindow();
            if (window.ShowDialog() == true)
                LoadSections();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedSection();
            if (selected != null)
            {
                var window = new AddEditSectionWindow(selected);
                if (window.ShowDialog() == true)
                    LoadSections();
            }
            else FeedbackService.Error("Выберите секцию!");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedSection();
            if (selected != null)
            {
                if (FeedbackService.Question("Вы уверены, что хотите удалить секцию?") == MessageBoxResult.Yes)
                {
                    selected.IsActive = false;
                    App.context.SaveChanges();
                    LoadSections();
                }
            }
            else FeedbackService.Error("Выберите секцию!");
        }

        private Sections GetSelectedSection()
        {
            var selectedItem = SectionListView.SelectedItem ?? _selectedListItem;
            if (selectedItem == null) return null;
            dynamic item = selectedItem;
            int id = item.Id;
            return App.context.Sections.Find(id);
        }
    }
}