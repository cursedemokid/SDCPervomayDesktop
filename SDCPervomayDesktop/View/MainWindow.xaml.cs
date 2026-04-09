using SDCPervomayDesktop.View;
using SDCPervomayDesktop.View.Pages;
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

namespace SDCPervomayDesktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new ClientsPage());
        }

        private void ClientsTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
        }

        private void ScheduleTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new SchedulePage());
        }

        private void KeysTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new KeysPage());
        }

        private void BackTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ExitTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
            Close();
        }
    }
}
