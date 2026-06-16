using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;
using Microsoft.Win32;

namespace Admissions_Reserve
{
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
            UpdateDbStatus();
        }

        private void UpdateDbStatus()
        {
            if (DatabaseHelper.IsDatabaseConnected)
            {
                DbStatusText.Text = "Подключена";
                DbStatusText.Foreground = System.Windows.Media.Brushes.Green;
                DbPathText.Text = $"Путь: {DatabaseHelper.CurrentDatabasePath}";
            }
            else
            {
                DbStatusText.Text = "Не подключена";
                DbStatusText.Foreground = System.Windows.Media.Brushes.Red;
                DbPathText.Text = "База данных не подключена. Нажмите 'Подключить базу данных'.";
            }
        }

        // Кнопка "Войти"
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;

                if (UserAuthentication.CurrentUserRole == "Admin")
                {
                    // Администратор (встроенный или из БД) → панель администратора
                    mainWindow?.ShowAdminPage();
                }
                else
                {
                    // Обычный пользователь – требуется БД
                    if (!DatabaseHelper.IsDatabaseConnected)
                    {
                        MessageBox.Show("База данных не подключена. Обратитесь к администратору или подключите БД.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    mainWindow?.MainFrame.Navigate(new View.ApplicantSearchPage());
                }
            }
        }

        // Кнопка "Подключить базу данных"
        private void BtnConnectDb_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл базы данных SQLite",
                Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*",
                DefaultExt = ".db"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DatabaseHelper.SwitchDatabase(dialog.FileName);
                    UpdateDbStatus();
                    MessageBox.Show($"База данных успешно подключена:\n{dialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Кнопка "Выход"
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}