using System;
using System.Windows.Controls;
using System.Windows;

namespace Admissions_Reserve.View
{
    public partial class ApplicantWizardPage : Page
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

        public ApplicantWizardPage()
        {
            InitializeComponent();
            NavigateToStep(_currentStep);
        }

        private void Step_Click(object sender, RoutedEventArgs e)
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
                StepFrame.Navigate(page);
        }
    }
}
