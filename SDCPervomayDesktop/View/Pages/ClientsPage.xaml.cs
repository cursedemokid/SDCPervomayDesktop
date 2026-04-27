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
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
        }

        private void ReceptionBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditClientWindow();
            window.ShowDialog();
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = App.context.Clients.Where(c => c.IsActive).ToList();
            ClientsListView.ItemsSource = clients.Select(c => new
            {
                c.FullName,
                PhoneNumber = c.Phone,
                BirthDate = c.BirthDate?.ToString("dd.MM.yyyy"),
                c.Id
            }).ToList();
        }
    }
}
