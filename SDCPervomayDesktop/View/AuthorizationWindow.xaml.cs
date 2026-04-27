using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.View
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void PasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                FeedbackService.Error("Введите логин и пароль", "Ошибка");
                return;
            }

            var employee = App.context.Employees
                .FirstOrDefault(emp => emp.Login == login && emp.IsActive);

            if (employee == null)
            {
                FeedbackService.Error("Неверный логин или пароль", "Ошибка");
                return;
            }

            if (!VerifyPassword(password, employee))
            {
                FeedbackService.Error("Неверный логин или пароль", "Ошибка");
                return;
            }

            App.CurrentEmployee = employee;
            MainWindow main = new MainWindow();
            main.Show();
            Close();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, Employees employee)
        {
            string stored = employee.PasswordHash;
            if (string.IsNullOrEmpty(stored))
                return false;

            bool isHashFormat = stored.Length == 44 && stored.EndsWith("=");

            if (isHashFormat)
            {
                return HashPassword(enteredPassword) == stored;
            }
            else
            {
                if (enteredPassword == stored)
                {
                    employee.PasswordHash = HashPassword(enteredPassword);
                    App.context.SaveChanges();
                    return true;
                }
                return false;
            }
        }
    }
}