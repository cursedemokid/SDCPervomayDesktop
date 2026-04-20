using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDCPervomayDesktop.Models
{
    public class Section : INotifyPropertyChanged
    {
        private int _id;
        private string _title;
        private string _description;
        private int _trainerId;
        private byte[] _imageData;
        private ObservableCollection<ScheduleItem> _schedule;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public int TrainerId { get => _trainerId; set { _trainerId = value; OnPropertyChanged(); } }
        public byte[] ImageData { get => _imageData; set { _imageData = value; OnPropertyChanged(); } }

        public ObservableCollection<ScheduleItem> Schedule
        {
            get => _schedule ?? (_schedule = new ObservableCollection<ScheduleItem>());
            set { _schedule = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}