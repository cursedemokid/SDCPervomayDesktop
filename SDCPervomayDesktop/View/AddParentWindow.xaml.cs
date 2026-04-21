using System.Windows;
using SDCPervomayDesktop.Models;
using SDCPervomayDesktop.AppData;

namespace SDCPervomayDesktop.View
{
    public partial class AddParentWindow : Window
    {
        public Client CreatedParent { get; private set; }

        public AddParentWindow()
        {
            InitializeComponent();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                FeedbackService.Error("Введите фамилию.", "Ошибка");
                return;
            }
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                FeedbackService.Error("Введите имя.", "Ошибка");
                return;
            }
            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                FeedbackService.Error("Введите номер телефона.", "Ошибка");
                return;
            }

            CreatedParent = new Client
            {
                LastName = LastNameBox.Text.Trim(),
                FirstName = FirstNameBox.Text.Trim(),
                Patronymic = PatronymicBox.Text?.Trim(),
                Phone = PhoneBox.Text.Trim(),
                // Роль родителя автоматически проставится в БД (RoleId = 5) 
                // или через отдельное поле IsParent = true, но у нас сущность Client общая.
                // В таблице Clients можно добавить поле IsParent BIT или использовать RoleId (Parent = 5).
            };

            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}