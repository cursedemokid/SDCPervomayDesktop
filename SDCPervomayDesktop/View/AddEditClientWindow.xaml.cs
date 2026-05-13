using SDCPervomayDesktop;
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
        private readonly ObservableCollection<Clients> parents;
        private readonly Clients _editClient;
        private readonly bool _parentQuickAdd;

        /// <summary>Заполнено после сохранения в режиме «только родитель».</summary>
        public Clients SavedParent { get; private set; }

        /// <summary>
        /// Добавление или редактирование посетителя (клиента).
        /// </summary>
        /// <param name="editClient">Клиент для правки или null для нового.</param>
        /// <param name="parentQuickAdd">Краткая форма добавления родителя (та же сущность Clients).</param>
        public AddEditClientWindow(Clients editClient = null, bool parentQuickAdd = false)
        {
            InitializeComponent();

            _editClient = editClient;
            _parentQuickAdd = parentQuickAdd;

            parents = new ObservableCollection<Clients>(
                App.context.Clients.Where(c => c.IsActive && c.IsParent).ToList()
            );
            GrowClientsCmb.ItemsSource = parents;
            GrowClientsCmb.DisplayMemberPath = "FullName";
            GrowClientsCmb.SelectedValuePath = "Id";

            IsYoung.Visibility = Visibility.Collapsed;

            if (_parentQuickAdd)
            {
                BirthDateGroup.Visibility = Visibility.Collapsed;
                IsYoung.Visibility = Visibility.Collapsed;
                PhoneNumberTb.IsEnabled = true;
                Title = "Добавить родителя";
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                SaveBtn.Content = "Добавить";
            }
            else if (_editClient != null)
            {
                Title = "Редактирование посетителя";
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                SaveBtn.Content = "Сохранить";

                LastNameTb.Text = _editClient.LastName;
                FirstNameTb.Text = _editClient.FirstName;
                PatronymicTb.Text = _editClient.Patronymic;
                BirthDateDp.SelectedDate = _editClient.BirthDate;
                PhoneNumberTb.Text = _editClient.Phone;
                if (_editClient.ParentId.HasValue)
                    GrowClientsCmb.SelectedValue = _editClient.ParentId.Value;
                UpdateParentVisibility();
            }
            else
            {
                Title = "Новый посетитель";
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
            }

            BirthDateDp.SelectedDateChanged += (s, e) => UpdateParentVisibility();
        }

        private void UpdateParentVisibility()
        {
            if (_parentQuickAdd)
                return;

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
            var dlg = new AddEditClientWindow(null, parentQuickAdd: true);
            if (dlg.ShowDialog() != true || dlg.SavedParent == null)
                return;

            parents.Clear();
            foreach (var p in App.context.Clients.Where(c => c.IsActive && c.IsParent).OrderBy(c => c.LastName).ThenBy(c => c.FirstName))
                parents.Add(p);
            GrowClientsCmb.SelectedValue = dlg.SavedParent.Id;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_parentQuickAdd)
            {
                if (string.IsNullOrWhiteSpace(LastNameTb.Text))
                {
                    FeedbackService.Error("Введите фамилию.", "Ошибка");
                    return;
                }
                if (string.IsNullOrWhiteSpace(FirstNameTb.Text))
                {
                    FeedbackService.Error("Введите имя.", "Ошибка");
                    return;
                }
                if (string.IsNullOrWhiteSpace(PhoneNumberTb.Text))
                {
                    FeedbackService.Error("Введите номер телефона.", "Ошибка");
                    return;
                }

                var parent = new Clients
                {
                    LastName = LastNameTb.Text.Trim(),
                    FirstName = FirstNameTb.Text.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(PatronymicTb.Text) ? null : PatronymicTb.Text.Trim(),
                    Phone = PhoneNumberTb.Text.Trim(),
                    Email = "",
                    IsParent = true,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                App.context.Clients.Add(parent);
                App.context.SaveChanges();
                SavedParent = parent;
                DialogResult = true;
                Close();
                return;
            }

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
