using System;
using System.Windows.Controls;
using System.Windows;
using Admissions_Reserve.View;

namespace Admissions_Reserve.Model
{
    /// <summary>
    /// Статический класс для управления навигацией в приложении
    /// </summary>
    public static class NavigationManager
    {
        private static Frame _mainFrame;
        private static ApplicantWizardPage _currentWizard;

        /// <summary>
        /// Инициализирует NavigationManager с главным Frame из MainWindow
        /// </summary>
        public static void Initialize(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        /// <summary>
        /// Устанавливает текущий ApplicantWizardPage для управления навигацией между шагами
        /// </summary>
        public static void SetCurrentWizard(ApplicantWizardPage wizard)
        {
            _currentWizard = wizard;
        }

        /// <summary>
        /// Переходит на следующий шаг в мастере
        /// </summary>
        public static void GoNextStep()
        {
            if (_currentWizard != null)
            {
                _currentWizard.GoNext();
            }
        }

        /// <summary>
        /// Переходит на предыдущий шаг в мастере
        /// </summary>
        public static void GoPreviousStep()
        {
            if (_currentWizard != null)
            {
                _currentWizard.GoBack();
            }
        }

        /// <summary>
        /// Навигирует на страницу поиска абитуриентов
        /// </summary>
        public static void GoToApplicantSearch()
        {
            if (_mainFrame != null)
            {
                _mainFrame.Navigate(new ApplicantSearchPage());
            }
        }

        /// <summary>
        /// Навигирует на страницу приветствия
        /// </summary>
        public static void GoToWelcome()
        {
            if (_mainFrame != null)
            {
                _mainFrame.Navigate(new WelcomePage());
            }
        }
    }
}
