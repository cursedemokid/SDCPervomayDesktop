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

        public SectionPage()
        {
            InitializeComponent();
            LoadSections();
            if (App.CurrentEmployee.RoleId == 3)
                AdminGrid.Visibility = Visibility.Collapsed;

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

            SectionListView.ItemsSource = filtered.Select(s => new
            {
                s.Title,
                s.Description,
                TrainerName = App.context.Employees.FirstOrDefault(e => e.Id == s.TrainerId)?.FullName,
                DaysInWeek = App.context.ScheduleItems.Count(si => si.SectionId == s.Id),
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
            if (SectionListView.SelectedItem == null) return null;
            dynamic item = SectionListView.SelectedItem;
            int id = item.Id;
            return App.context.Sections.Find(id);
        }
    }
}