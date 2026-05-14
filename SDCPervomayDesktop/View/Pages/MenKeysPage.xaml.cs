using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SDCPervomayDesktop.ViewModels;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class MenKeysPage : Page
    {
        public MenKeysPage()
        {
            InitializeComponent();
            DataContext = new MenKeysPageViewModel(true);
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
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                FontSize = 20,
                Background = Brushes.White,
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