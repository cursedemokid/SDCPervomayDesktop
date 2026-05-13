using System.ComponentModel;
using System.Runtime.CompilerServices;
using SDCPervomayDesktop.Entities;

namespace SDCPervomayDesktop.ViewModels
{
    /// <summary>
    /// UI-обёртка над сущностью шкафчика из БД: цвет кнопки и уведомления об изменении состояния.
    /// </summary>
    public class LockerViewModel : INotifyPropertyChanged
    {
        public LockerViewModel(Lockers entity)
        {
            Entity = entity ?? throw new System.ArgumentNullException(nameof(entity));
        }

        public Lockers Entity { get; }

        public int Id => Entity.Id;

        public string Number => Entity.Number;

        public bool IsOccupied
        {
            get => Entity.IsOccupied;
            set
            {
                if (Entity.IsOccupied == value)
                    return;

                Entity.IsOccupied = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonColor));
                OnPropertyChanged(nameof(ContextMenuItemText));
            }
        }

        public string ButtonColor => IsOccupied ? "#FFE6B3" : "#FFFFFF";

        public string ContextMenuItemText => IsOccupied ? "Вернуть ключ" : "Выдать ключи";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
