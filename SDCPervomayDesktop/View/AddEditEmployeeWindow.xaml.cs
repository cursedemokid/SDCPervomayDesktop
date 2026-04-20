using Microsoft.Win32;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static SDCPervomayDesktop.AppData.SystemEnums;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditEmployeeWindow : Window
    {
        private bool _isEditMode;
        private Employee _currentEmployee;
        private ObservableCollection<Achievement> _achievements;

        public AddEditEmployeeWindow(Employee employeeToEdit = null)
        {
            InitializeComponent();

            LoadRoles();

            if (employeeToEdit != null)
            {
                _isEditMode = true;
                _currentEmployee = employeeToEdit;
                LoadEmployeeData();
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                Title = $"Редактирование сотрудника: {_currentEmployee.FullName}";
            }
            else
            {
                _isEditMode = false;
                _currentEmployee = new Employee();
                _achievements = new ObservableCollection<Achievement>();
                AchievementsListBox.ItemsSource = _achievements;
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление сотрудника";
            }
        }

        private void LoadRoles()
        {
            var roles = new ObservableCollection<Role>
            {
                new Role { Id = 1, Name = "Тренер" },
                new Role { Id = 2, Name = "Администратор" },
                new Role { Id = 3, Name = "Медицинский персонал" }
            };
            RoleCombo.ItemsSource = roles;
            RoleCombo.SelectedValuePath = "Id";
            RoleCombo.DisplayMemberPath = "Name";
        }

        private void LoadEmployeeData()
        {
            LastNameBox.Text = _currentEmployee.LastName;
            FirstNameBox.Text = _currentEmployee.FirstName;
            PatronymicBox.Text = _currentEmployee.Patronymic;
            BirthDatePicker.SelectedDate = _currentEmployee.BirthDate;
            PhoneBox.Text = _currentEmployee.Phone;
            EmailBox.Text = _currentEmployee.Email;
            RoleCombo.SelectedValue = _currentEmployee.RoleId;
            SalaryBox.Text = _currentEmployee.Salary.ToString();

            _achievements = new ObservableCollection<Achievement>(_currentEmployee.Achievements ?? new ObservableCollection<Achievement>());
            AchievementsListBox.ItemsSource = _achievements;

            if (_currentEmployee.PhotoData != null && _currentEmployee.PhotoData.Length > 0)
                EmployeeImage.Source = ByteArrayToBitmapImage(_currentEmployee.PhotoData);
        }

        private void AddAchievement_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAchievementDialog();
            if (dialog.ShowDialog() == true)
            {
                _achievements.Add(new Achievement { Text = dialog.AchievementText });
            }
        }

        private void EditAchievement_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var achievement = button?.Tag as Achievement;
            if (achievement == null) return;

            var dialog = new AddAchievementDialog(achievement.Text);
            if (dialog.ShowDialog() == true)
            {
                achievement.Text = dialog.AchievementText;
                AchievementsListBox.Items.Refresh();
            }
        }

        private void DeleteAchievement_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var achievement = button?.Tag as Achievement;
            if (achievement == null) return;

            if (FeedbackService.Question($"Удалить заслугу \"{achievement.Text}\"?") == MessageBoxResult.Yes)
            {
                _achievements.Remove(achievement);
            }
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            if (openDialog.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(openDialog.FileName);
                _currentEmployee.PhotoData = imageData;
                EmployeeImage.Source = ByteArrayToBitmapImage(imageData);
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            UpdateEmployeeFromForm();
            FeedbackService.Information($"Сотрудник {_currentEmployee.FullName} добавлен!", "Успех");
            DialogResult = true;
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            UpdateEmployeeFromForm();
            FeedbackService.Information("Изменения сохранены!", "Успех");
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                FeedbackService.Error("Введите фамилию.", "Ошибка");
                return false;
            }
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                FeedbackService.Error("Введите имя.", "Ошибка");
                return false;
            }
            if (RoleCombo.SelectedValue == null)
            {
                FeedbackService.Error("Выберите должность.", "Ошибка");
                return false;
            }
            if (!decimal.TryParse(SalaryBox.Text, out _))
            {
                FeedbackService.Error("Введите корректную зарплату.", "Ошибка");
                return false;
            }
            return true;
        }

        private void UpdateEmployeeFromForm()
        {
            _currentEmployee.LastName = LastNameBox.Text;
            _currentEmployee.FirstName = FirstNameBox.Text;
            _currentEmployee.Patronymic = PatronymicBox.Text;
            _currentEmployee.BirthDate = BirthDatePicker.SelectedDate;
            _currentEmployee.Phone = PhoneBox.Text;
            _currentEmployee.Email = EmailBox.Text;
            _currentEmployee.RoleId = (int)RoleCombo.SelectedValue;
            _currentEmployee.Salary = decimal.Parse(SalaryBox.Text);
            _currentEmployee.Achievements = _achievements;
        }

        private BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            return bitmap;
        }
    }
}