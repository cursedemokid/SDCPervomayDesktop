using SDCPervomayDesktop;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using SDCPervomayDesktop.View;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class ClientsPage : Page
    {
        private List<Clients> allClients;
        private object _selectedListItem;

        public ClientsPage()
        {
            InitializeComponent();
            FilterCombo.SelectedIndex = 0;
            LoadClients();
            if (App.CurrentEmployee.RoleId == 3)
                AdminGrid.Visibility = Visibility.Collapsed;
            else
                ReceptionBtn.Visibility = Visibility.Collapsed;

            ClientsListView.SelectionChanged += (s, e) =>
            {
                if (ClientsListView.SelectedItem != null)
                    _selectedListItem = ClientsListView.SelectedItem;
            };

            FilterCombo.SelectionChanged += (s, e) => ApplyFilters();
            SearchTextBox.TextChanged += (s, e) => ApplyFilters();
        }

        private void LoadClients()
        {
            allClients = App.context.Clients.Where(c => c.IsActive).ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = allClients.AsEnumerable();

            // Возрастной фильтр
            if (FilterCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                string tag = selectedItem.Content.ToString();
                if (tag == "Все")
                {
                    // без возрастного фильтра
                }
                else
                {
                    DateTime today = DateTime.Today;
                    if (tag == "Дети до 18")
                        filtered = filtered.Where(c => c.BirthDate.HasValue && (today.Year - c.BirthDate.Value.Year - (today.DayOfYear < c.BirthDate.Value.DayOfYear ? 1 : 0)) < 18);
                    else if (tag == "Старше 18")
                        filtered = filtered.Where(c => !c.BirthDate.HasValue || (today.Year - c.BirthDate.Value.Year - (today.DayOfYear < c.BirthDate.Value.DayOfYear ? 1 : 0)) >= 18);
                }
            }

            // Поиск по ФИО
            string search = SearchTextBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(c => (c.FullName ?? "").ToLower().Contains(search));

            ClientsListView.ItemsSource = filtered.Select(c => new
            {
                c.FullName,
                PhoneNumber = c.Phone,
                BirthDate = c.BirthDate?.ToString("dd.MM.yyyy"),
                c.Id
            }).ToList();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e) => ApplyFilters();

        private void ClientsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditBtn_Click(sender, e);
        }

        private void ReceptionBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditClientWindow();
            if (window.ShowDialog() == true)
                LoadClients();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditClientWindow();
            if (window.ShowDialog() == true)
                LoadClients();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected != null)
            {
                var window = new AddEditClientWindow(selected);
                if (window.ShowDialog() == true)
                    LoadClients();
            }
            else FeedbackService.Error("Выберите посетителя!");
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedClient();
            if (selected != null)
            {
                if (FeedbackService.Question("Вы уверены, что хотите удалить посетителя?") == MessageBoxResult.Yes)
                {
                    selected.IsActive = false;
                    App.context.SaveChanges();
                    LoadClients();
                }
            }
            else FeedbackService.Error("Выберите посетителя!");
        }

        private Clients GetSelectedClient()
        {
            var selectedItem = ClientsListView.SelectedItem ?? _selectedListItem;
            if (selectedItem == null) return null;
            dynamic item = selectedItem;
            int id = item.Id;
            return App.context.Clients.Find(id);
        }
    }
}