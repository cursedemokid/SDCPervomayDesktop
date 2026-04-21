using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SDCPervomayDesktop.ViewModels;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class UnifiedKeysPage : Page
    {
        public UnifiedKeysPage()
        {
            InitializeComponent();
            MaleToggle.Checked += GenderToggle_Checked;
            FemaleToggle.Checked += GenderToggle_Checked;
        }

        private void GenderToggle_Checked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle == MaleToggle)
            {
                MaleGrid.Visibility = Visibility.Visible;
                FemaleGrid.Visibility = Visibility.Collapsed;
                MaleToggle.IsChecked = true;
                FemaleToggle.IsChecked = false;
            }
            else if (toggle == FemaleToggle)
            {
                MaleGrid.Visibility = Visibility.Collapsed;
                FemaleGrid.Visibility = Visibility.Visible;
                MaleToggle.IsChecked = false;
                FemaleToggle.IsChecked = true;
            }
        }

        private void LockerButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var locker = button?.Tag as Locker;
            if (locker == null) return;

            var popup = new Popup
            {
                PlacementTarget = button,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

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

            var freeItem = new ComboBoxItem { Content = "Свободен", Tag = false };
            var occupiedItem = new ComboBoxItem { Content = "Занят", Tag = true };
            comboBox.Items.Add(freeItem);
            comboBox.Items.Add(occupiedItem);

            comboBox.SelectedIndex = locker.IsOccupied ? 1 : 0;

            comboBox.SelectionChanged += (s, args) =>
            {
                var selected = comboBox.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    bool newStatus = (bool)selected.Tag;
                    if (locker.IsOccupied != newStatus)
                        locker.IsOccupied = newStatus;
                }
                popup.IsOpen = false;
            };

            popup.Child = comboBox;
            popup.IsOpen = true;
        }
    }
}