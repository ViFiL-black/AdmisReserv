using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Admissions_Reserve.View;
using Admissions_Reserve.Model;

namespace Admissions_Reserve
{
    public partial class MainWindow : Window
    {
        private readonly string[] _stepTags = new[]
        {
            "IdentityPage",
            "ContactsPage",
            "ApplicationTypeAndEducationPage",
            "DocumentsPage",
            "RelativesPage",
            "AdditionalInfoPage",
            "ApplicationCompetitionsPage",
            "IndividualAchievementsPage",
            "PrioritiesPage",
            "AttachedDocumentsPage"
        };
        private int _currentStep = 0;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new WelcomePage());
            MainFrame.Navigated += MainFrame_Navigated;
            NavigationManager.Initialize(MainFrame);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationMenu.Visibility = Visibility.Collapsed;

            if (e.Content is ApplicantWizardPage)
            {
                NavigationMenu.Visibility = Visibility.Visible;
                if (e.Content is ApplicantWizardPage wizard)
                    NavigationManager.SetCurrentWizard(wizard);
            }
            else if (IsDataEntryPage(e.Content))
            {
                NavigationMenu.Visibility = Visibility.Visible;
            }
            else if (e.Content is AdminPage)
            {
                NavigationMenu.Visibility = Visibility.Collapsed; // для админки меню скрываем
            }
            else
            {
                NavigationMenu.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsDataEntryPage(object content)
        {
            var dataEntryPages = new[]
            {
                typeof(IdentityPage),
                typeof(ContactsPage),
                typeof(ApplicationTypeAndEducationPage),
                typeof(DocumentsPage),
                typeof(RelativesPage),
                typeof(AdditionalInfoPage),
                typeof(ApplicationCompetitionsPage),
                typeof(IndividualAchievementsPage),
                typeof(PrioritiesPage),
                typeof(AttachedDocumentsPage)
            };
            return dataEntryPages.Contains(content?.GetType());
        }

        private void NavigationMenu_StepClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                int idx = Array.IndexOf(_stepTags, tag);
                if (idx >= 0)
                {
                    _currentStep = idx;
                    NavigateToStep(_currentStep);
                }
            }
        }

        private void NavigateToStep(int step)
        {
            var currentApplicant = SessionManager.CurrentApplicant;
            if (step != 0 && (currentApplicant == null || currentApplicant.Id == 0))
            {
                MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                _currentStep = 0;
                MainFrame.Navigate(new IdentityPage());
                return;
            }

            Page page = null;
            switch (step)
            {
                case 0:
                    page = new IdentityPage();
                    break;
                case 1:
                    page = new ContactsPage();
                    break;
                case 2:
                    page = new ApplicationTypeAndEducationPage();
                    break;
                case 3:
                    page = new DocumentsPage();
                    break;
                case 4:
                    page = new RelativesPage();
                    break;
                case 5:
                    page = new AdditionalInfoPage();
                    break;
                case 6:
                    page = new ApplicationCompetitionsPage();
                    break;
                case 7:
                    page = new IndividualAchievementsPage();
                    break;
                case 8:
                    page = new PrioritiesPage();
                    break;
                case 9:
                    page = new AttachedDocumentsPage();
                    break;
            }

            if (page != null)
                MainFrame.Navigate(page);
        }

        /// <summary>
        /// Открыть страницу администратора (вызывается из WelcomePage после успешного входа админа)
        /// </summary>
        public void ShowAdminPage()
        {
            SessionManager.CurrentApplicant = null;
            MainFrame.Navigate(new AdminPage());
        }
    }
}