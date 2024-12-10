using System;
using System.Linq;
using System.Windows;

namespace KnigKing
{
    /// <summary>
    /// Логика взаимодействия для Autoriz.xaml
    /// </summary>
    public partial class Autoriz : Window
    {
        GlEntities gl = new GlEntities();

        public Autoriz()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = log.Text;
            string password = pas.Password;

            // Поиск пользователя в базе данных по логину и паролю
            var user = gl.Polzs.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                CurrentUser.UserId = user.ID;
                CurrentUser.UserRole = user.Role.Name;

                if (user.ID_Role == 2)  // Если роль - менеджер
                {
                    ManagerWindow managerWindow = new ManagerWindow();
                    managerWindow.Show();
                }
                else if (user.ID_Role == 3)  // Если роль - администратор
                {
                    AdminWindow managerWindow = new AdminWindow();
                    managerWindow.Show();
                }
                else
                {
                    // Open MainWindow for other roles
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }

                // Close the authorization window
                this.Close();
            }
            else
            {
                // If user not found, show an error message
                MessageBox.Show("Неверный логин или пароль!", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}