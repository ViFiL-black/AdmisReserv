using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve
{
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        // Кнопка "Войти" (в XAML называется BtnSearch)
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;

                if (UserAuthentication.CurrentUserRole == "Admin")
                {
                    // Администратор → страница управления пользователями
                    mainWindow?.ShowAdminPage();
                }
                else
                {
                    // Обычный пользователь → страница заполнения анкеты абитуриента
                    mainWindow?.MainFrame.Navigate(new View.ApplicantSearchPage());
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