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
    /// Логика взаимодействия для SchedulePage.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        public SchedulePage()
        {
            InitializeComponent();
        }

        private void MondayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TuesdayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void WednesdayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ThursdayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FridayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SaturdayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SundayTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddEditScheduleWindow window = new AddEditScheduleWindow();
            window.ShowDialog();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
