using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Linq;
using SDCPervomayDesktop.Models;
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

            var vm = DataContext as MenKeysPageViewModel;
            if (vm == null) return;

            var tooltipPanel = new StackPanel
            {
                Width = 260,
                Margin = new Thickness(8)
            };

            var titleText = new TextBlock
            {
                Text = $"История: {locker.Number}",
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 6)
            };
            tooltipPanel.Children.Add(titleText);

            var historyText = new TextBlock
            {
                Text = locker.History.Any()
                    ? string.Join("\n", locker.History
                        .OrderByDescending(h => h.EventTime)
                        .Take(5)
                        .Select(h => $"{h.EventTime:dd.MM.yyyy HH:mm} - {h.Action}"))
                    : "История пока пуста",
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            tooltipPanel.Children.Add(historyText);

            var separator = new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 8)
            };
            tooltipPanel.Children.Add(separator);

            if (locker.IsOccupied)
            {
                var returnButton = new Button
                {
                    Content = "Вернуть ключ",
                    Width = 220,
                    Height = 28,
                    Background = Brushes.White,
                    Foreground = Brushes.Black,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    FontSize = 14
                };
                tooltipPanel.Children.Add(returnButton);

                returnButton.Click += (s, args) =>
                {
                    locker.IsOccupied = false;
                    locker.History.Add(new LockerHistory
                    {
                        LockerNumber = locker.Number,
                        Action = "Ключ принят на ресепшне"
                    });

                    if (button.ToolTip is ToolTip openedToolTip)
                    {
                        openedToolTip.IsOpen = false;
                    }
                };
            }
            else
            {
                var hintText = new TextBlock
                {
                    Text = "Выберите, кому выдать ключ",
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0, 0, 0, 6)
                };
                tooltipPanel.Children.Add(hintText);

                var visitorComboBox = new ComboBox
                {
                    Width = 220,
                    Height = 25,
                    DisplayMemberPath = nameof(Visitor.FullName),
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    FontSize = 14,
                    Background = Brushes.White,
                    Foreground = Brushes.Black,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    ItemsSource = vm.Visitors,
                    SelectedIndex = -1
                };
                tooltipPanel.Children.Add(visitorComboBox);

                visitorComboBox.SelectionChanged += (s, args) =>
                {
                    var selectedVisitor = visitorComboBox.SelectedItem as Visitor;
                    if (selectedVisitor == null)
                    {
                        return;
                    }

                    selectedVisitor.ArrivalTime = System.DateTime.Now;
                    selectedVisitor.AssignedLockerNumber = locker.Number;
                    locker.Visitors.Add(selectedVisitor);
                    locker.IsOccupied = true;
                    locker.History.Add(new LockerHistory
                    {
                        LockerNumber = locker.Number,
                        Action = $"Ключ выдан посетителю: {selectedVisitor.FullName}"
                    });

                    if (button.ToolTip is ToolTip openedToolTip)
                    {
                        openedToolTip.IsOpen = false;
                    }
                };
            }

            var tooltip = new ToolTip
            {
                Content = tooltipPanel,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                PlacementTarget = button,
                Placement = PlacementMode.Bottom,
                StaysOpen = false
            };

            button.ToolTip = tooltip;
            tooltip.IsOpen = true;
        }
    }
}