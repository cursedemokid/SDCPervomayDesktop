using SDCPervomayDesktop.ViewModels;
using SDCPervomayDesktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SDCPervomayDesktop.ViewModels
{
    public class MenKeysPageViewModel
    {
        private readonly bool _isMaleLockerRoom;

        public ObservableCollection<Locker> UpperLockers { get; set; } // Ш10-Ш18
        public ObservableCollection<Locker> LowerLockers { get; set; } // Ш01-Ш09
        public ObservableCollection<Reminder> Reminders { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Visitor> Visitors { get; set; }

        public MenKeysPageViewModel() : this(true)
        {
        }

        public MenKeysPageViewModel(bool isMaleLockerRoom)
        {
            _isMaleLockerRoom = isMaleLockerRoom;
            UpperLockers = new ObservableCollection<Locker>();
            LowerLockers = new ObservableCollection<Locker>();
            Reminders = new ObservableCollection<Reminder>();
            Products = new ObservableCollection<Product>();
            Visitors = new ObservableCollection<Visitor>();

            // Заполняем верхний ряд (Ш10-Ш18)
            string[] upperNumbers = { "Ш10", "Ш11", "Ш12", "Ш13", "Ш014", "Ш15", "Ш16", "Ш17", "Ш18" };
            foreach (var num in upperNumbers)
                UpperLockers.Add(new Locker { Number = num, IsOccupied = false });

            // Заполняем нижний ряд (Ш01-Ш09)
            string[] lowerNumbers = { "Ш01", "Ш02", "Ш03", "Ш04", "Ш05", "Ш06", "Ш07", "Ш08", "Ш09" };
            foreach (var num in lowerNumbers)
                LowerLockers.Add(new Locker { Number = num, IsOccupied = false });

            Reminders.Add(new Reminder
            {
                Title = "Обновить ассортимент",
                Description = "Проверить и пополнить спортивные товары на полках.",
                DueTime = DateTime.Today.AddHours(12),
                Category = "Ассортимент"
            });

            Reminders.Add(new Reminder
            {
                Title = "Уборка зоны ресепшна",
                Description = "Проверить чистоту зоны шкафчиков и стойки администратора.",
                DueTime = DateTime.Today.AddHours(18),
                Category = "Уборка"
            });

            Products.Add(new Product
            {
                Name = "Эспандер",
                Category = "Фитнес",
                Quantity = 8,
                ShelfCode = "ПЛ-01",
                HasExpirationDate = false
            });
            Products.Add(new Product
            {
                Name = "Энергетический батончик",
                Category = "Питание",
                Quantity = 12,
                ShelfCode = "ПЛ-02",
                HasExpirationDate = true,
                ExpirationDate = DateTime.Today.AddDays(14)
            });
            Products.Add(new Product
            {
                Name = "Изотоник",
                Category = "Напитки",
                Quantity = 2,
                ShelfCode = "ПЛ-03",
                HasExpirationDate = true,
                ExpirationDate = DateTime.Today.AddDays(3)
            });

            if (_isMaleLockerRoom)
            {
                Visitors.Add(new Visitor { FullName = "Иван Петров", Gender = "Мужской", ArrivalTime = DateTime.Now.AddMinutes(-20) });
                Visitors.Add(new Visitor { FullName = "Артем Смирнов", Gender = "Мужской", ArrivalTime = DateTime.Now.AddMinutes(-5) });
                Visitors.Add(new Visitor { FullName = "Дмитрий Орлов", Gender = "Мужской", ArrivalTime = DateTime.Now.AddMinutes(-2) });
            }
            else
            {
                Visitors.Add(new Visitor { FullName = "Мария Соколова", Gender = "Женский", ArrivalTime = DateTime.Now.AddMinutes(-10) });
                Visitors.Add(new Visitor { FullName = "Анна Кузнецова", Gender = "Женский", ArrivalTime = DateTime.Now.AddMinutes(-7) });
                Visitors.Add(new Visitor { FullName = "Елена Воронцова", Gender = "Женский", ArrivalTime = DateTime.Now.AddMinutes(-3) });
            }
        }

        // Команда для действия (выдать/вернуть ключ)
        public ICommand LockerActionCommand => new RelayCommand<Locker>(ExecuteLockerAction);

        private void ExecuteLockerAction(Locker locker)
        {
            // Переключаем состояние
            locker.IsOccupied = !locker.IsOccupied;

            // Здесь можно добавить логику сохранения в базу данных, лог и т.п.
            // Например, показать уведомление:
            // MessageBox.Show($"Шкафчик {locker.Number} теперь {(locker.IsOccupied ? "занят" : "свободен")}");
        }
    }

    // Простая реализация RelayCommand
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