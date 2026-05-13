using SDCPervomayDesktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для WomanKeysPage.xaml
    /// </summary>
    public partial class WomanKeysPage : Page
    {
        public WomanKeysPage()
        {
            InitializeComponent();
        }

        private void LockerButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var locker = button?.Tag as LockerViewModel;
            if (locker == null) return;

            // Создаём всплывающее окно (Popup)
            var popup = new Popup
            {
                PlacementTarget = button,          // привязываем к кнопке
                Placement = PlacementMode.Bottom,  // появляется снизу
                StaysOpen = false,                 // закроется при клике вне
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            // Создаём ComboBox
            var comboBox = new ComboBox
            {
                Width = 100,
                Height = 25,
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                FontSize = 14,
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            // Добавляем два статуса
            var freeItem = new ComboBoxItem { Content = "Свободен", Tag = false };
            var occupiedItem = new ComboBoxItem { Content = "Занят", Tag = true };
            comboBox.Items.Add(freeItem);
            comboBox.Items.Add(occupiedItem);

            // Выбираем текущий статус
            comboBox.SelectedIndex = locker.IsOccupied ? 1 : 0;

            // Обработчик выбора
            comboBox.SelectionChanged += (s, args) =>
            {
                var selected = comboBox.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    bool newStatus = (bool)selected.Tag;
                    if (locker.IsOccupied != newStatus)
                    {
                        locker.IsOccupied = newStatus; // цвет обновится автоматически
                    }
                }
                popup.IsOpen = false; // закрываем после выбора
            };

            popup.Child = comboBox;
            popup.IsOpen = true;
        }
    }
}
