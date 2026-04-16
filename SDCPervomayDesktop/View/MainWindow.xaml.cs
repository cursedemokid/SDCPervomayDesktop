using SDCPervomayDesktop.View;
using SDCPervomayDesktop.View.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDCPervomayDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Открываем страницу посетителей по умолчанию
            MainFrame.Navigate(new ClientsPage());
            UpdateMenuUnderline(typeof(ClientsPage));
        }

        private void ClientsTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
            UpdateMenuUnderline(typeof(ClientsPage));
        }

        private void ScheduleTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new SchedulePage());
            UpdateMenuUnderline(typeof(SchedulePage));
        }

        private void KeysTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new MenKeysPage());
            UpdateMenuUnderline(typeof(MenKeysPage));
        }

        private void BackTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
                // Обновляем подчёркивание в зависимости от текущей страницы
                var currentPage = MainFrame.Content as Page;
                if (currentPage != null)
                {
                    UpdateMenuUnderline(currentPage.GetType());
                }
            }
        }

        private void ExitTbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
            Close();
        }

        // Метод для обновления подчёркивания выбранного пункта меню
        private void UpdateMenuUnderline(Type pageType)
        {
            // Сбрасываем подчёркивание у всех
            ClientsTbl.TextDecorations = null;
            ScheduleTbl.TextDecorations = null;
            KeysTbl.TextDecorations = null;

            // Устанавливаем подчёркивание для активной страницы
            if (pageType == typeof(ClientsPage))
                ClientsTbl.TextDecorations = TextDecorations.Underline;
            else if (pageType == typeof(SchedulePage))
                ScheduleTbl.TextDecorations = TextDecorations.Underline;
            else if (pageType == typeof(MenKeysPage))
                KeysTbl.TextDecorations = TextDecorations.Underline;
        }
    }
}