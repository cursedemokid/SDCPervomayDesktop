using SDCPervomayDesktop.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.ViewModels
{
    public class MenKeysPageViewModel
    {
        private readonly bool _isMaleLockerRoom;

        public ObservableCollection<Lockers> UpperLockers { get; set; }
        public ObservableCollection<Lockers> LowerLockers { get; set; }
        public ObservableCollection<Reminders> Reminders { get; set; }
        public ObservableCollection<Products> Products { get; set; }
        public ObservableCollection<Visitors> Visitors { get; set; }

        public MenKeysPageViewModel() : this(true)
        {
        }

        public MenKeysPageViewModel(bool isMaleLockerRoom)
        {
            _isMaleLockerRoom = isMaleLockerRoom;
            UpperLockers = new ObservableCollection<Lockers>();
            LowerLockers = new ObservableCollection<Lockers>();
            Reminders = new ObservableCollection<Reminders>();
            Products = new ObservableCollection<Products>();
            Visitors = new ObservableCollection<Visitors>();

            string[] upperNumbers = { "Ш10", "Ш11", "Ш12", "Ш13", "Ш014", "Ш15", "Ш16", "Ш17", "Ш18" };
            foreach (var num in upperNumbers)
                UpperLockers.Add(new Lockers { Number = num, IsOccupied = false });

            string[] lowerNumbers = { "Ш01", "Ш02", "Ш03", "Ш04", "Ш05", "Ш06", "Ш07", "Ш08", "Ш09" };
            foreach (var num in lowerNumbers)
                LowerLockers.Add(new Lockers { Number = num, IsOccupied = false });

            Reminders.Add(new Reminders
            {
                Title = "Обновить ассортимент",
                Description = "Проверить и пополнить спортивные товары на полках.",
                DueTime = DateTime.Today.AddHours(12),
                Category = "Ассортимент"
            });

            Reminders.Add(new Reminders
            {
                Title = "Уборка зоны ресепшна",
                Description = "Проверить чистоту зоны шкафчиков и стойки администратора.",
                DueTime = DateTime.Today.AddHours(18),
                Category = "Уборка"
            });

            Products.Add(new Products
            {
                Name = "Эспандер",
                Category = "Фитнес",
                Quantity = 8,
                ShelfCode = "ПЛ-01",
                HasExpirationDate = false
            });
            Products.Add(new Products
            {
                Name = "Энергетический батончик",
                Category = "Питание",
                Quantity = 12,
                ShelfCode = "ПЛ-02",
                HasExpirationDate = true,
                ExpirationDate = DateTime.Today.AddDays(14)
            });
            Products.Add(new Products
            {
                Name = "Изотоник",
                Category = "Напитки",
                Quantity = 2,
                ShelfCode = "ПЛ-03",
                HasExpirationDate = true,
                ExpirationDate = DateTime.Today.AddDays(3)
            });
        }

        public ICommand LockerActionCommand => new RelayCommand<Lockers>(ExecuteLockerAction);

        private void ExecuteLockerAction(Lockers locker)
        {
            locker.IsOccupied = !locker.IsOccupied;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}