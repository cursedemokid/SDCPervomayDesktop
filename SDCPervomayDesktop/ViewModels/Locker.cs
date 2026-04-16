using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDCPervomayDesktop.ViewModels
{
    public class Locker : INotifyPropertyChanged
    {
        private bool _isOccupied;
        public string Number { get; set; }

        public bool IsOccupied
        {
            get => _isOccupied;
            set
            {
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