using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SDCPervomayDesktop;
using SDCPervomayDesktop.AppData;
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

            RefreshLockerAssigneeLists();
        }

        private void CloseActiveLockerPopup()
        {
            if (_activeLockerPopup == null)
                return;

            _activeLockerPopup.IsOpen = false;
            _activeLockerPopup = null;
        }

        private void RefreshLockerAssigneeLists()
        {
            maleViewModel.ActiveVisitors = new ObservableCollection<ActiveVisitorListItem>(
                LoadLockerAssignees(isMaleRoom: true));
            femaleViewModel.ActiveVisitors = new ObservableCollection<ActiveVisitorListItem>(
                LoadLockerAssignees(isMaleRoom: false));
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

        private bool ClientMatchesLockerRoom(int clientId, bool isMaleRoom)
        {
            var genders = App.context.Visitors
                .Where(v => v.ClientId == clientId)
                .Select(v => v.Gender)
                .ToList();

            if (genders.Count == 0)
                return true;

            return genders.Any(g => MatchesLockerRoomGender(g, isMaleRoom));
        }

        private List<ActiveVisitorListItem> LoadLockerAssignees(bool isMaleRoom)
        {
            return App.context.Clients
                .Where(c => c.IsActive && !c.IsParent)
                .ToList()
                .Where(c => ClientMatchesLockerRoom(c.Id, isMaleRoom))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c =>
                {
                    var visitor = App.context.Visitors
                        .Where(v => v.ClientId == c.Id)
                        .ToList()
                        .Where(v => MatchesLockerRoomGender(v.Gender, isMaleRoom))
                        .OrderByDescending(v => v.ArrivalTime)
                        .FirstOrDefault();

                    return new ActiveVisitorListItem
                    {
                        Id = visitor?.Id ?? 0,
                        ClientId = c.Id,
                        FullName = GetClientDisplayName(c) ?? "Неизвестный клиент"
                    };
                })
                .ToList();
        }

        private bool IsMaleRoomActive() =>
            MaleGrid != null && MaleGrid.Visibility == Visibility.Visible;

        private static bool CanManageKeys() =>
            App.CurrentEmployee != null &&
            App.CurrentEmployee.RoleId == (int)SystemEnums.Role.Worker;

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

            if (!CanManageKeys())
            {
                tooltipPanel.Children.Add(new TextBlock
                {
                    Text = "Просмотр. Выдача и возврат ключей доступны только сотруднику ресепшена.",
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else if (locker.IsOccupied)
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
                    RefreshLockerAssigneeLists();

                    CloseActiveLockerPopup();
                };
            }
            else
            {
                var hintText = new TextBlock
                {
                    Text = "Выберите посетителя",
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0, 0, 0, 6)
                };
                tooltipPanel.Children.Add(hintText);

                var isMaleRoom = IsMaleRoomActive();
                var assignees = LoadLockerAssignees(isMaleRoom);

                var visitorComboBox = new ComboBox
                {
                    Width = 220,
                    MinHeight = 28,
                    Padding = new Thickness(8, 2, 8, 2),
                    FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                    FontSize = 14,
                    DisplayMemberPath = "FullName",
                    ItemsSource = assignees,
                    SelectedIndex = -1
                };
                tooltipPanel.Children.Add(visitorComboBox);

                if (assignees.Count == 0)
                {
                    visitorComboBox.IsEnabled = false;
                    tooltipPanel.Children.Add(new TextBlock
                    {
                        Text = isMaleRoom
                            ? "Нет клиентов для мужской раздевалки"
                            : "Нет клиентов для женской раздевалки",
                        FontFamily = (FontFamily)Application.Current.Resources["GothamProFamily"],
                        Foreground = Brushes.Gray,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 6, 0, 0)
                    });
                }

                visitorComboBox.SelectionChanged += (s, args) =>
                {
                    var selected = visitorComboBox.SelectedItem as ActiveVisitorListItem;
                    if (selected == null)
                        return;

                    locker.CurrentClientId = selected.ClientId;
                    lockerVm.IsOccupied = true;

                    var visitorEntity = selected.Id > 0
                        ? App.context.Visitors.Find(selected.Id)
                        : null;
                    visitorEntity = visitorEntity
                        ?? App.context.Visitors
                            .Where(v => v.ClientId == selected.ClientId)
                            .ToList()
                            .Where(v => MatchesLockerRoomGender(v.Gender, isMaleRoom))
                            .OrderByDescending(v => v.ArrivalTime)
                            .FirstOrDefault();

                    if (visitorEntity == null)
                    {
                        visitorEntity = new Visitors
                        {
                            ClientId = selected.ClientId,
                            Gender = isMaleRoom ? "М" : "Ж",
                            ArrivalTime = DateTime.Now
                        };
                        App.context.Visitors.Add(visitorEntity);
                    }

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
                    RefreshLockerAssigneeLists();

                    CloseActiveLockerPopup();
                };
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
