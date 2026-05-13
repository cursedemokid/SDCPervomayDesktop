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
    public partial class EmployeesPage : Page
    {
        private List<Employees> allEmployees;

        public EmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
            if (App.CurrentEmployee.RoleId == 3)
                AdminGrid.Visibility = Visibility.Collapsed;

            SearchTextBox.TextChanged += (s, e) => ApplyFilter();
        }

        private void LoadEmployees()
        {
            allEmployees = App.context.Employees.Where(e => e.IsActive).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string search = SearchTextBox.Text?.Trim().ToLower();
            var filtered = allEmployees;
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(e =>
                    (e.FullName ?? "").ToLower().Contains(search) ||
                    (e.Email ?? "").ToLower().Contains(search) ||
                    (e.Phone ?? "").ToLower().Contains(search) ||
                    (e.Login ?? "").ToLower().Contains(search)
                ).ToList();

            EmployeeListView.ItemsSource = filtered.Select(e => new
            {
                e.FullName,
                e.Email,
                PhoneNumber = e.Phone,
                e.Login,
                Role = App.context.Roles.FirstOrDefault(r => r.Id == e.RoleId)?.Name,
                BirthDate = e.BirthDate?.ToString("dd.MM.yyyy"),
                e.Id
            }).ToList();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditEmployeeWindow();
            if (window.ShowDialog() == true)
                LoadEmployees();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedEmployee();
            if (selected != null)
            {
                var window = new AddEditEmployeeWindow(selected);
                if (window.ShowDialog() == true)
                    LoadEmployees();
            }
            else FeedbackService.Error("Выберите сотрудника!");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedEmployee();
            if (selected != null)
            {
                if (FeedbackService.Question("Вы уверены, что хотите удалить сотрудника?") == MessageBoxResult.Yes)
                {
                    selected.IsActive = false;
                    App.context.SaveChanges();
                    LoadEmployees();
                }
            }
            else FeedbackService.Error("Выберите сотрудника!");
        }

        private Employees GetSelectedEmployee()
        {
            if (EmployeeListView.SelectedItem == null) return null;
            dynamic item = EmployeeListView.SelectedItem;
            int id = item.Id;
            return App.context.Employees.Find(id);
        }
    }
}