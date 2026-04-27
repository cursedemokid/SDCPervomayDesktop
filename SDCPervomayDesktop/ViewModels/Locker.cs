using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.ViewModels
{
    public class Locker : INotifyPropertyChanged
    {
        private bool _isOccupied;
        public string Number { get; set; }
        public string CurrentVisitorFullName { get; set; }
        public ObservableCollection<LockerHistory> History { get; } = new ObservableCollection<LockerHistory>();
        public ObservableCollection<Visitors> Visitors { get; } = new ObservableCollection<Visitors>();
        public ObservableCollection<Products> Products { get; } = new ObservableCollection<Products>();

        public bool IsOccupied
        {
            get => _isOccupied;
            set
            {
                if (_isOccupied == value)
                {
                    return;
                }

                _isOccupied = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonColor));
                OnPropertyChanged(nameof(ContextMenuItemText));
            }
        }

        // Цвет кнопки в зависимости от состояния
        public string ButtonColor => IsOccupied ? "#FFE6B3" : "#FFFFFF"; // занят – светло-жёлтый, свободен – белый

        // Текст пункта меню
        public string ContextMenuItemText => IsOccupied ? "Вернуть ключ" : "Выдать ключи";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}