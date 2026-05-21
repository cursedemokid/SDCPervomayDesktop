using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SDCPervomayDesktop;
using SDCPervomayDesktop.Entities;
using SDCPervomayDesktop.ViewModels;

namespace SDCPervomayDesktop.View.Pages
{
    public partial class UnifiedKeysPage : Page
    {
        private readonly MenKeysPageViewModel maleViewModel;
        private readonly MenKeysPageViewModel femaleViewModel;
        private Popup _activeLockerPopup;

        public UnifiedKeysPage()
        {
            InitializeComponent();

            maleViewModel = new MenKeysPageViewModel(true);
            femaleViewModel = new MenKeysPageViewModel(false);
            DataContext = maleViewModel;
            MaleToggle.IsChecked = true;

            LoadVisitors();
        }

        private void CloseActiveLockerPopup()
        {
            if (_activeLockerPopup == null)
                return;

            _activeLockerPopup.IsOpen = false;
            _activeLockerPopup = null;
        }

        private void LoadVisitors()
        {
            maleViewModel.ActiveVisitors = new ObservableCollection<ActiveVisitorListItem>(
                LoadActiveVisitors(isMaleRoom: true));
            femaleViewModel.ActiveVisitors = new ObservableCollection<ActiveVisitorListItem>(
                LoadActiveVisitors(isMaleRoom: false));
        }

        private static bool MatchesLockerRoomGender(string visitorGender, bool isMaleRoom)
        {
            var g = (visitorGender ?? "").Trim();
            if (string.IsNullOrEmpty(g))
                return true;

            if (isMaleRoom)
                return g == "М" || g == "M" || g == "м" || g == "m";

            return g == "Ж" || g == "F" || g == "ж" || g == "f";
        }

        private List<ActiveVisitorListItem> LoadActiveVisitors(bool isMaleRoom, bool onlyWithoutLocker = false)
        {
            var visitors = App.context.Visitors
                .Where(v => v.DepartureTime == null)
                .ToList()
                .Where(v => MatchesLockerRoomGender(v.Gender, isMaleRoom));

            if (onlyWithoutLocker)
                visitors = visitors.Where(v => v.AssignedLockerId == null).ToList();

            return visitors.Select(v =>
            {
                var client = v.Clients ?? App.context.Clients.Find(v.ClientId);
                return new ActiveVisitorListItem
                {
                    Id = v.Id,
                    ClientId = v.ClientId,
                    FullName = GetClientDisplayName(client) ?? "Неизвестный клиент"
                };
            }).ToList();
        }

        private bool IsMaleRoomActive() =>
            MaleGrid != null && MaleGrid.Visibility == Visibility.Visible;

        private void MaleToggle_Checked(object sender, RoutedEventArgs e)
        {
            CloseActiveLockerPopup();
            if (FemaleToggle != null)
                FemaleToggle.IsChecked = false;

            DataContext = maleViewModel;
            if (MaleGrid != null)
                MaleGrid.Visibility = Visibility.Visible;
            if (FemaleGrid != null)
                FemaleGrid.Visibility = Visibility.Collapsed;
        }

        private void FemaleToggle_Checked(object sender, RoutedEventArgs e)
        {
            CloseActiveLockerPopup();
            if (MaleToggle != null)
                MaleToggle.IsChecked = false;

            DataContext = femaleViewModel;
            if (MaleGrid != null)
                MaleGrid.Visibility = Visibility.Collapsed;
            if (FemaleGrid != null)
                FemaleGrid.Visibility = Visibility.Visible;
        }

        private void LockerButton_Click(object sender, RoutedEventArgs e)
        {
            CloseActiveLockerPopup();

            var button = sender as Button;
            var lockerVm = button?.Tag as LockerViewModel;
            if (lockerVm == null && button?.Tag != null)
            {
                var lockerId = button.Tag is int id ? id : int.TryParse(button.Tag.ToString(), out var parsed) ? parsed : 0;
                if (lockerId > 0)
                    lockerVm = FindLockerViewModel(lockerId);
            }
            if (lockerVm == null) return;

            var locker = lockerVm.Entity;

            var history = GetLockerHistory(locker.Id);
            var tooltipPanel = new StackPanel
            {
                Width = 260,
                Margin = new Thickness(8)
            };

            var titleText = new TextBlock
            {
                Text = $"Шкафчик: {locker.Number}",
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 6)
            };
            tooltipPanel.Children.Add(titleText);

            var historyText = new TextBlock
            {
                Text = BuildHistoryText(history),
                FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };

            var historyScroll = new ScrollViewer
            {
                Content = historyText,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 180,
                Margin = new Thickness(0, 0, 0, 8)
            };
            tooltipPanel.Children.Add(historyScroll);

            var separator = new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 8)
            };
            tooltipPanel.Children.Add(separator);

            Popup lockerPopup = null;

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
                    locker.CurrentClientId = null;
                    lockerVm.IsOccupied = false;

                    var visitor = App.context.Visitors.FirstOrDefault(v => v.AssignedLockerId == locker.Id && v.DepartureTime == null);
                    if (visitor != null)
                        visitor.AssignedLockerId = null;

                    App.context.LockerHistory.Add(new LockerHistory
                    {
                        LockerId = locker.Id,
                        ActionType = "Returned",
                        EventTime = DateTime.Now,
                        Comment = "Ключ возвращён"
                    });
                    App.context.SaveChanges();
                    LoadVisitors();

                    CloseActiveLockerPopup();
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

                var visitors = LoadActiveVisitors(IsMaleRoomActive(), onlyWithoutLocker: true);
                if (visitors.Count == 0)
                {
                    tooltipPanel.Children.Add(new TextBlock
                    {
                        Text = "Нет активных посетителей без ключа",
                        FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                        Foreground = Brushes.Gray,
                        TextWrapping = TextWrapping.Wrap
                    });
                }
                else
                {
                    var visitorComboBox = new ComboBox
                    {
                        Width = 220,
                        MinHeight = 28,
                        Padding = new Thickness(8, 2, 8, 2),
                        FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                        FontSize = 14,
                        DisplayMemberPath = "FullName",
                        ItemsSource = visitors,
                        SelectedIndex = -1
                    };
                    tooltipPanel.Children.Add(visitorComboBox);

                    visitorComboBox.SelectionChanged += (s, args) =>
                    {
                        var selected = visitorComboBox.SelectedItem as ActiveVisitorListItem;
                        if (selected == null)
                            return;

                        locker.CurrentClientId = selected.ClientId;
                        lockerVm.IsOccupied = true;

                        var visitorEntity = App.context.Visitors.Find(selected.Id);
                        if (visitorEntity != null)
                            visitorEntity.AssignedLockerId = locker.Id;

                        App.context.LockerHistory.Add(new LockerHistory
                        {
                            LockerId = locker.Id,
                            ClientId = selected.ClientId,
                            ActionType = "Issued",
                            EventTime = DateTime.Now,
                            Comment = $"Ключ выдан: {selected.FullName}"
                        });
                        App.context.SaveChanges();
                        LoadVisitors();

                        CloseActiveLockerPopup();
                    };
                }
            }

            var panelBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = tooltipPanel
            };

            lockerPopup = new Popup
            {
                Child = panelBorder,
                PlacementTarget = button,
                Placement = PlacementMode.Bottom,
                StaysOpen = true,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };
            lockerPopup.Closed += (s, args) =>
            {
                if (ReferenceEquals(_activeLockerPopup, lockerPopup))
                    _activeLockerPopup = null;
            };

            _activeLockerPopup = lockerPopup;
            lockerPopup.IsOpen = true;
        }

        private LockerViewModel FindLockerViewModel(int lockerId)
        {
            var vm = DataContext as MenKeysPageViewModel;
            if (vm == null) return null;
            return vm.UpperLockers.Concat(vm.LowerLockers).FirstOrDefault(l => l.Id == lockerId);
        }

        private List<LockerHistory> GetLockerHistory(int lockerId)
        {
            return App.context.LockerHistory
                .Where(h => h.LockerId == lockerId)
                .OrderByDescending(h => h.EventTime)
                .ToList();
        }

        private string BuildHistoryText(List<LockerHistory> history)
        {
            if (history.Count == 0)
                return "История пуста";

            var lines = new List<string>();
            foreach (var h in history)
            {
                string action = h.ActionType == "Issued" ? "Выдан" : "Возвращён";
                string clientName = h.ClientId != null
                    ? GetClientDisplayName(App.context.Clients.FirstOrDefault(c => c.Id == h.ClientId))
                    ?? "Клиент"
                    : "";
                lines.Add($"[{h.EventTime:dd.MM.yyyy HH:mm}] {action}: {clientName}");
            }
            return string.Join("\n", lines);
        }

        private static string GetClientDisplayName(Clients client)
        {
            if (client == null)
                return null;

            return (client.LastName + " " + client.FirstName + " " + (client.Patronymic ?? "")).Trim();
        }
    }
}
