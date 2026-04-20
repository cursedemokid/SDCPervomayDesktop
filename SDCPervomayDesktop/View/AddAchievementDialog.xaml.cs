using System.Windows;

namespace SDCPervomayDesktop.View
{
    public partial class AddAchievementDialog : Window
    {
        public string AchievementText { get; private set; }

        public AddAchievementDialog(string existingText = null)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(existingText))
                AchievementTextBox.Text = existingText;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AchievementTextBox.Text))
            {
                MessageBox.Show("Введите текст заслуги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            AchievementText = AchievementTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}