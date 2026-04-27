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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDCPervomayDesktop.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeesPage.xaml
    /// </summary>
    public partial class EmployeesPage : Page
    {
        public EmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var employees = App.context.Employees
                .Where(e => e.IsActive)
                .ToList();
            SectionListView.ItemsSource = employees.Select(e => new
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

        // EditBtn, DeleteBtn – аналогично, передача Id
    }
}
