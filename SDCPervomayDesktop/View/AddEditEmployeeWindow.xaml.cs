using Microsoft.Win32;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static SDCPervomayDesktop.AppData.SystemEnums;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditEmployeeWindow : Window
    {
        private bool _isEditMode;
        private Employees _currentEmployee;
        private ObservableCollection<Achievements> _achievements;

        public AddEditEmployeeWindow(Employees employeeToEdit = null)
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
                _currentEmployee = new Employees();
                _achievements = new ObservableCollection<Achievements>();
                AchievementsListBox.ItemsSource = _achievements;
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление сотрудника";
            }
        }

        private void LoadRoles()
        {
            var roles = Enum.GetValues(typeof(Role))
                .Cast<Role>()
                .Select(r => new { Id = (int)r, Name = r.ToString() })
                .ToList();
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

            _achievements = new ObservableCollection<Achievements>(_currentEmployee.Achievements ?? new ObservableCollection<Achievements>());
            AchievementsListBox.ItemsSource = _achievements;

            if (_currentEmployee.PhotoData != null && _currentEmployee.PhotoData.Length > 0)
                EmployeeImage.Source = ByteArrayToBitmapImage(_currentEmployee.PhotoData);
        }

        private void AddAchievement_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAchievementDialog();
            if (dialog.ShowDialog() == true)
            {
                _achievements.Add(new Achievements { Text = dialog.AchievementText });
            }
        }

        private void EditAchievement_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var achievement = button?.Tag as Achievements;
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
            var achievement = button?.Tag as Achievements;
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

            if (string.IsNullOrWhiteSpace(_currentEmployee.Login))
                _currentEmployee.Login = GenerateLogin();
            if (string.IsNullOrWhiteSpace(_currentEmployee.PasswordHash))
                _currentEmployee.PasswordHash = HashPassword("123456");

            _currentEmployee.CreatedAt = DateTime.Now;
            _currentEmployee.IsActive = true;

            App.context.Employees.Add(_currentEmployee);
            SyncAchievements();
            App.context.SaveChanges();

            FeedbackService.Information($"Сотрудник {_currentEmployee.FullName} добавлен!", "Успех");
            DialogResult = true;
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            UpdateEmployeeFromForm();
            SyncAchievements();
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
        }

        private void SyncAchievements()
        {
            if (_achievements == null) return;

            var existing = (_currentEmployee.Achievements ?? Enumerable.Empty<Achievements>()).ToList();
            foreach (var exist in existing)
            {
                if (!_achievements.Any(a => a.Id == exist.Id))
                    App.context.Achievements.Remove(exist);
            }

            foreach (var a in _achievements)
            {
                if (a.Id == 0)
                {
                    a.Employees = _currentEmployee;
                    a.CreatedAt = DateTime.Now;
                    App.context.Achievements.Add(a);
                }
            }
        }

        private string GenerateLogin()
        {
            string translit = Transliterate(_currentEmployee.LastName + _currentEmployee.FirstName);
            return translit.Length > 20 ? translit.Substring(0, 20) : translit;
        }

        private string Transliterate(string text)
        {
            string[] rus = { "а","б","в","г","д","е","ё","ж","з","и","й","к","л","м","н","о","п","р","с","т","у","ф","х","ц","ч","ш","щ","ъ","ы","ь","э","ю","я",
                             "А","Б","В","Г","Д","Е","Ё","Ж","З","И","Й","К","Л","М","Н","О","П","Р","С","Т","У","Ф","Х","Ц","Ч","Ш","Щ","Ъ","Ы","Ь","Э","Ю","Я" };
            string[] eng = { "a","b","v","g","d","e","yo","zh","z","i","y","k","l","m","n","o","p","r","s","t","u","f","kh","ts","ch","sh","shch",null,"y",null,"e","yu","ya",
                             "A","B","V","G","D","E","Yo","Zh","Z","I","Y","K","L","M","N","O","P","R","S","T","U","F","Kh","Ts","Ch","Sh","Shch",null,"Y",null,"E","Yu","Ya" };
            for (int i = 0; i < rus.Length; i++)
                text = text.Replace(rus[i], eng[i]);
            return text;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
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