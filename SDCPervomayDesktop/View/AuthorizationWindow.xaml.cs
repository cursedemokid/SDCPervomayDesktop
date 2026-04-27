using SDCPervomayDesktop.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDCPervomayDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
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
            // Хеширование пароля (например, SHA256) – в реальном проекте должно быть единообразно
            string passwordHash = HashPassword(password);
            var employee = App.context.Employees
                .FirstOrDefault(emp => emp.Login == login && emp.PasswordHash == passwordHash && emp.IsActive);
            if (employee == null)
            {
                FeedbackService.Error("Неверный логин или пароль", "Ошибка");
                return;
            }
            App.CurrentEmployee = employee;
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
