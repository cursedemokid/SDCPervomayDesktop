using SDCPervomayDesktop.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SDCPervomayDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для AddEditClientWindow.xaml
    /// </summary>
    public partial class AddEditClientWindow : Window
    {
        private ObservableCollection<Clients> parents;
        public AddEditClientWindow(Clients editClient = null)
        {
            InitializeComponent();
            parents = new ObservableCollection<Clients>(
                App.context.Clients.Where(c => c.IsActive && c.IsParent).ToList()
            );
            GrowClientsCmb.ItemsSource = parents;
            GrowClientsCmb.DisplayMemberPath = "FullName";
            GrowClientsCmb.SelectedValuePath = "Id";

            if (editClient != null)
            {
                
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var client = new Clients
            {
                LastName = LastNameTb.Text,
                FirstName = FirstNameTb.Text,
                Patronymic = PatronymicTb.Text,
                BirthDate = BirthDateDp.SelectedDate,
                Phone = PhoneNumberTb.IsEnabled ? PhoneNumberTb.Text : null,
                Email = "",
                ParentId = (int?)GrowClientsCmb.SelectedValue
            };
            App.context.Clients.Add(client);
            App.context.SaveChanges();
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateParentVisibility()
        {
            var birthDate = BirthDateDp.SelectedDate;
            if (birthDate == null) return;
            int age = DateTime.Today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;

            bool isYoung = age < 18;
            IsYoung.Visibility = isYoung ? Visibility.Visible : Visibility.Collapsed;
            PhoneNumberTb.IsEnabled = !isYoung;
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
    }
}
