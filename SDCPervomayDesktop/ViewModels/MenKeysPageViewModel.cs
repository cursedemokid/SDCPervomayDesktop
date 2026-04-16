using SDCPervomayDesktop.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SDCPervomayDesktop.ViewModels
{
    public class MenKeysPageViewModel
    {
        public ObservableCollection<Locker> UpperLockers { get; set; } // Ш10-Ш18
        public ObservableCollection<Locker> LowerLockers { get; set; } // Ш01-Ш09

        public MenKeysPageViewModel()
        {
            UpperLockers = new ObservableCollection<Locker>();
            LowerLockers = new ObservableCollection<Locker>();

            // Заполняем верхний ряд (Ш10-Ш18)
            string[] upperNumbers = { "Ш10", "Ш11", "Ш12", "Ш13", "Ш014", "Ш15", "Ш16", "Ш17", "Ш18" };
            foreach (var num in upperNumbers)
                UpperLockers.Add(new Locker { Number = num, IsOccupied = false });

            // Заполняем нижний ряд (Ш01-Ш09)
            string[] lowerNumbers = { "Ш01", "Ш02", "Ш03", "Ш04", "Ш05", "Ш06", "Ш07", "Ш08", "Ш09" };
            foreach (var num in lowerNumbers)
                LowerLockers.Add(new Locker { Number = num, IsOccupied = false });
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