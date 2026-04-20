using Microsoft.Win32;
using SDCPervomayDesktop.AppData;
using SDCPervomayDesktop.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SDCPervomayDesktop.View
{
    public partial class AddEditSectionWindow : Window
    {
        private bool _isEditMode;
        private Section _currentSection;
        public ObservableCollection<Trainer> Trainers { get; set; }

        public AddEditSectionWindow(Section sectionToEdit = null)
        {
            InitializeComponent();
            DataContext = this;

            LoadTrainers();

            if (sectionToEdit != null)
            {
                _isEditMode = true;
                _currentSection = sectionToEdit;
                LoadSectionData();
                AddBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Visible;
                Title = "Редактирование секции";
            }
            else
            {
                _isEditMode = false;
                _currentSection = new Section();
                AddBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Collapsed;
                Title = "Добавление секции";
            }
        }

        private void LoadTrainers()
        {
            // Статические данные (можно вынести в общий сервис)
            Trainers = new ObservableCollection<Trainer>
            {
                new Trainer { Id = 1, FullName = "Иванов Иван Иванович" },
                new Trainer { Id = 2, FullName = "Петров Петр Петрович" },
                new Trainer { Id = 3, FullName = "Сидорова Анна Владимировна" }
            };
            TrainerCombo.ItemsSource = Trainers;
        }

        private void LoadSectionData()
        {
            TitleTextBox.Text = _currentSection.Title;
            DescTextBox.Text = _currentSection.Description;
            TrainerCombo.SelectedValue = _currentSection.TrainerId;
            if (_currentSection.ImageData != null && _currentSection.ImageData.Length > 0)
                PreviewImage.Source = ByteArrayToBitmapImage(_currentSection.ImageData);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            UpdateSectionFromForm();
            // Здесь сохранить в БД
            FeedbackService.Information($"Секция \"{_currentSection.Title}\" добавлена!", "Успех");
            DialogResult = true;
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            UpdateSectionFromForm();
            FeedbackService.Information("Изменения сохранены!", "Успех");
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            if (openDialog.ShowDialog() == true)
            {
                byte[] imageData = File.ReadAllBytes(openDialog.FileName);
                _currentSection.ImageData = imageData;
                PreviewImage.Source = ByteArrayToBitmapImage(imageData);
            }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                FeedbackService.Error("Введите название секции.", "Ошибка");
                return false;
            }
            if (TrainerCombo.SelectedValue == null)
            {
                FeedbackService.Error("Выберите тренера.", "Ошибка");
                return false;
            }
            return true;
        }

        private void UpdateSectionFromForm()
        {
            _currentSection.Title = TitleTextBox.Text;
            _currentSection.Description = DescTextBox.Text;
            _currentSection.TrainerId = (int)TrainerCombo.SelectedValue;
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
    }
}