using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDCPervomayDesktop.Models
{
    public class Training : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private int _trainerId;
        private byte[] _imageData;

        public int Id { get; set; }
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public int TrainerId { get => _trainerId; set { _trainerId = value; OnPropertyChanged(); } }
        public byte[] ImageData { get => _imageData; set { _imageData = value; OnPropertyChanged(); } }

        public ObservableCollection<ScheduleItem> Schedule { get; set; } = new ObservableCollection<ScheduleItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}