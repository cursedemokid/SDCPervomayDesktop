using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SDCPervomayDesktop.Models;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditScheduleWindow : Window
    {
        // Режим окна: true - редактирование, false - добавление
        private bool _isEditMode;
        private Training _currentTraining;

        // Коллекции для привязки
        public ObservableCollection<Trainer> Trainers { get; set; }
        public ObservableCollection<ScheduleItem> ScheduleItems { get; set; }

        public AddEditScheduleWindow(Training trainingToEdit = null)
        {
            InitializeComponent();
            DataContext = this;

            // Загружаем статический список тренеров
            LoadTrainers();

            // Инициализируем коллекцию расписания
            ScheduleItems = new ObservableCollection<ScheduleItem>();
            ScheduleListBox.ItemsSource = ScheduleItems;

            if (trainingToEdit != null)
            {
                // Режим редактирования
                _isEditMode = true;
                _currentTraining = trainingToEdit;
                LoadTrainingData();
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                Title = "Редактирование занятия";
            }
            else
            {
                // Режим добавления
                _isEditMode = false;
                _currentTraining = new Training();
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление занятия";
            }
        }

        private void LoadTrainers()
        {
            // Статические данные тренеров
            Trainers = new ObservableCollection<Trainer>
            {
                new Trainer { Id = 1, FullName = "Иванов Иван Иванович" },
                new Trainer { Id = 2, FullName = "Петров Петр Петрович" },
                new Trainer { Id = 3, FullName = "Сидорова Анна Владимировна" }
            };
        }

        private void LoadTrainingData()
        {
            // Заполняем поля формы из переданного объекта
            var titleTextBox = (TextBox)((GroupBox)this.FindName("TitleGroupBox")).Content;
            titleTextBox.Text = _currentTraining.Title;

            var descTextBox = (TextBox)((GroupBox)this.FindName("DescGroupBox")).Content;
            descTextBox.Text = _currentTraining.Description;

            var trainerCombo = (ComboBox)((GroupBox)this.FindName("TrainerGroupBox")).Content;
            trainerCombo.SelectedValue = _currentTraining.TrainerId;

            // Заполняем расписание
            ScheduleItems.Clear();
            foreach (var item in _currentTraining.Schedule)
                ScheduleItems.Add(item);

            // Загружаем изображение, если есть
            if (_currentTraining.ImageData != null && _currentTraining.ImageData.Length > 0)
            {
                var image = (Image)this.FindName("PreviewImage");
                image.Source = ByteArrayToBitmapImage(_currentTraining.ImageData);
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // Режим добавления: собираем данные и сохраняем новую запись
            if (!ValidateFields()) return;

            UpdateTrainingFromForm();
            // Здесь обычно вызов сервиса для сохранения в БД
            MessageBox.Show($"Занятие \"{_currentTraining.Title}\" добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Режим редактирования
            if (!ValidateFields()) return;

            UpdateTrainingFromForm();
            MessageBox.Show($"Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddDayBtn_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог добавления дня недели и времени
            var dialog = new AddScheduleItemWindow();
            if (dialog.ShowDialog() == true)
            {
                ScheduleItems.Add(dialog.ScheduleItem);
            }
        }

        private void UpdateTrainingFromForm()
        {
            var titleTextBox = (TextBox)((GroupBox)this.FindName("TitleGroupBox")).Content;
            var descTextBox = (TextBox)((GroupBox)this.FindName("DescGroupBox")).Content;
            var trainerCombo = (ComboBox)((GroupBox)this.FindName("TrainerGroupBox")).Content;

            _currentTraining.Title = titleTextBox.Text;
            _currentTraining.Description = descTextBox.Text;
            _currentTraining.TrainerId = (int)trainerCombo.SelectedValue;
            _currentTraining.Schedule.Clear();
            foreach (var item in ScheduleItems)
                _currentTraining.Schedule.Add(item);
        }

        private bool ValidateFields()
        {
            var titleTextBox = (TextBox)((GroupBox)this.FindName("TitleGroupBox")).Content;
            if (string.IsNullOrWhiteSpace(titleTextBox.Text))
            {
                MessageBox.Show("Введите название занятия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var trainerCombo = (ComboBox)((GroupBox)this.FindName("TrainerGroupBox")).Content;
            if (trainerCombo.SelectedValue == null)
            {
                MessageBox.Show("Выберите тренера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ScheduleItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один день в расписание.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void AddImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            if (openDialog.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(openDialog.FileName);
                _currentTraining.ImageData = imageData;
                var image = (Image)this.FindName("PreviewImage");
                image.Source = ByteArrayToBitmapImage(imageData);
            }
        }

        private BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            return bitmap;
        }

        // Вспомогательный метод для поиска элементов по имени (упрощает код)
        private T FindControl<T>(string name) where T : FrameworkElement
        {
            return (T)this.FindName(name);
        }

        private void EditScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItem;
            if (item == null) return;

            // Открыть диалог редактирования, передав текущий item
            var dialog = new AddScheduleItemWindow(item);
            if (dialog.ShowDialog() == true)
            {
                // Обновить данные в существующем объекте
                item.DayName = dialog.ScheduleItem.DayName;
                item.StartTime = dialog.ScheduleItem.StartTime;
                item.EndTime = dialog.ScheduleItem.EndTime;
                // Обновить отображение (коллекция ObservableCollection сама обновит ListBox)
                var index = ScheduleItems.IndexOf(item);
                ScheduleItems[index] = item; // принудительное обновление, если нужно
                ScheduleListBox.Items.Refresh();
            }
        }

        private void DeleteScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ScheduleItem;
            if (item == null) return;

            if (MessageBox.Show($"Удалить расписание для {item.DayName}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ScheduleItems.Remove(item);
            }
        }
    }
}