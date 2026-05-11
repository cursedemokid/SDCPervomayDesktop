using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditClientWindow : Window
    {
        private ObservableCollection<Clients> parents;
        private Clients _editClient;

        public AddEditClientWindow(Clients editClient = null)
        {
            InitializeComponent();

            parents = new ObservableCollection<Clients>(
                App.context.Clients.Where(c => c.IsActive && c.IsParent).ToList()
            );
            GrowClientsCmb.ItemsSource = parents;
            GrowClientsCmb.DisplayMemberPath = "FullName";
            GrowClientsCmb.SelectedValuePath = "Id";

            IsYoung.Visibility = Visibility.Collapsed; // панель родителя скрыта

            BirthDateDp.SelectedDateChanged += (s, e) => UpdateParentVisibility();

            if (editClient != null)
            {
                _editClient = editClient;
                // Заполняем поля для редактирования
                LastNameTb.Text = editClient.LastName;
                FirstNameTb.Text = editClient.FirstName;
                PatronymicTb.Text = editClient.Patronymic;
                BirthDateDp.SelectedDate = editClient.BirthDate;
                PhoneNumberTb.Text = editClient.Phone;
                if (editClient.ParentId.HasValue)
                    GrowClientsCmb.SelectedValue = editClient.ParentId.Value;
                // Обновляем видимость родителя
                UpdateParentVisibility();
            }
        }

        private void UpdateParentVisibility()
        {
            var birthDate = BirthDateDp.SelectedDate;
            if (birthDate == null)
            {
                IsYoung.Visibility = Visibility.Collapsed;
                return;
            }

            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age)) age--;

            if (age < 18)
            {
                IsYoung.Visibility = Visibility.Visible;
                PhoneNumberTb.IsEnabled = false;
            }
            else
            {
                IsYoung.Visibility = Visibility.Collapsed;
                PhoneNumberTb.IsEnabled = true;
            }
        }

        private void AddParent_Click(object sender, RoutedEventArgs e)
        {
            var addParent = new AddParentWindow();
            if (addParent.ShowDialog() == true)
            {
                var newParent = addParent.CreatedParent;
                newParent.IsParent = true;
                App.context.Clients.Add(newParent);
                App.context.SaveChanges();
                parents.Add(newParent);
                GrowClientsCmb.SelectedValue = newParent.Id;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Валидация (можно добавить)
            if (string.IsNullOrWhiteSpace(LastNameTb.Text) || string.IsNullOrWhiteSpace(FirstNameTb.Text))
            {
                FeedbackService.Error("Фамилия и имя обязательны!");
                return;
            }

            Clients client;
            if (_editClient != null)
            {
                client = _editClient;
            }
            else
            {
                client = new Clients
                {
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                App.context.Clients.Add(client);
            }

            client.LastName = LastNameTb.Text;
            client.FirstName = FirstNameTb.Text;
            client.Patronymic = PatronymicTb.Text;
            client.BirthDate = BirthDateDp.SelectedDate;
            client.Phone = PhoneNumberTb.IsEnabled ? PhoneNumberTb.Text : null;
            client.Email = client.Email ?? "";
            client.ParentId = IsYoung.Visibility == Visibility.Visible ? (int?)GrowClientsCmb.SelectedValue : null;

            App.context.SaveChanges();
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