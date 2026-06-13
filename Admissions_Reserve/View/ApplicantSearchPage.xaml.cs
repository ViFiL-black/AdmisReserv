using Admissions_Reserve.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Admissions_Reserve.View
{
    public partial class ApplicantSearchPage : Page
    {
        private ObservableCollection<Applicants> searchResults;
        private Applicants selectedApplicant;

        public ApplicantSearchPage()
        {
            InitializeComponent();
            searchResults = new ObservableCollection<Applicants>();
            SearchResultsGrid.ItemsSource = searchResults;

            // Загружаем всех абитуриентов при открытии страницы
            LoadAllApplicants();
        }

        /// <summary>
        /// Загрузка всех абитуриентов из базы данных
        /// </summary>
        private void LoadAllApplicants()
        {
            try
            {
                var allApplicants = DataService.GetAll<Applicants>()
                    .OrderBy(a => a.LastName)
                    .ThenBy(a => a.FirstName)
                    .ToList();

                searchResults.Clear();
                foreach (var applicant in allApplicants)
                {
                    searchResults.Add(applicant);
                }

                if (allApplicants.Count == 0)
                    SearchStatusText.Text = "Нет зарегистрированных абитуриентов";
                else if (allApplicants.Count == 1)
                    SearchStatusText.Text = "Всего 1 абитуриент";
                else
                    SearchStatusText.Text = $"Всего {allApplicants.Count} абитуриентов";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка абитуриентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SearchStatusText.Text = "Ошибка загрузки данных";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                string lastName = LastNameSearchBox.Text?.Trim() ?? "";
                string firstName = FirstNameSearchBox.Text?.Trim() ?? "";
                string patronymic = PatronymicSearchBox.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(patronymic))
                {
                    // Если поля пустые – показываем всех
                    LoadAllApplicants();
                    return;
                }

                var allApplicants = DataService.GetAll<Applicants>();

                var results = allApplicants.Where(a =>
                {
                    bool exactMatch = false;
                    bool partialMatch = false;

                    if (!string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(a.LastName))
                    {
                        if (a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.LastName.IndexOf(lastName, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false;
                    }

                    if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(a.FirstName))
                    {
                        if (a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.FirstName.IndexOf(firstName, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false;
                    }

                    if (!string.IsNullOrWhiteSpace(patronymic) && !string.IsNullOrWhiteSpace(a.Patronymic))
                    {
                        if (a.Patronymic.Equals(patronymic, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.Patronymic.IndexOf(patronymic, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false;
                    }

                    return exactMatch || partialMatch;
                }).OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToList();

                searchResults.Clear();
                foreach (var applicant in results)
                    searchResults.Add(applicant);

                if (results.Count == 0)
                    SearchStatusText.Text = "Похожих абитуриентов не найдено";
                else if (results.Count == 1)
                    SearchStatusText.Text = "Найден 1 абитуриент";
                else
                    SearchStatusText.Text = $"Найдено {results.Count} абитуриентов";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                SearchStatusText.Text = "Ошибка при поиске";
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.Logout();
            SessionManager.CurrentApplicant = null;
            (Application.Current.MainWindow as MainWindow)?.MainFrame.Navigate(new WelcomePage());
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            LastNameSearchBox.Text = "";
            FirstNameSearchBox.Text = "";
            PatronymicSearchBox.Text = "";
            // После очистки показываем всех
            LoadAllApplicants();
            LastNameSearchBox.Focus();
        }

        private void SearchResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedApplicant = SearchResultsGrid.SelectedItem as Applicants;
        }

        private void SearchResultsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedApplicant != null)
                OpenApplicant(selectedApplicant, editMode: false);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicant = button?.Tag as Applicants;
            if (applicant != null)
                OpenApplicant(applicant, editMode: true);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicant = button?.Tag as Applicants;
            if (applicant != null)
                DeleteApplicant(applicant);
        }

        private void OpenApplicant(Applicants applicant, bool editMode)
        {
            try
            {
                SessionManager.CurrentApplicant = applicant;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    var wizardPage = new ApplicantWizardPage();
                    mainWindow.MainFrame.Navigate(wizardPage);
                }
                else
                {
                    NavigationService?.Navigate(new ApplicantWizardPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteApplicant(Applicants applicant)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить абитуриента {applicant.LastName} {applicant.FirstName}? " +
                    $"\r\nЭто действие необратимо и удалит все связанные данные.",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DataService.DeleteApplicant(applicant.Id);
                    searchResults.Remove(applicant);
                    selectedApplicant = null;

                    MessageBox.Show($"Абитуриент {applicant.LastName} {applicant.FirstName} успешно удален",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (searchResults.Count == 0)
                        SearchStatusText.Text = "Нет зарегистрированных абитуриентов";
                    else if (searchResults.Count == 1)
                        SearchStatusText.Text = "Всего 1 абитуриент";
                    else
                        SearchStatusText.Text = $"Всего {searchResults.Count} абитуриентов";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении абитуриента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void GenerateApplication_Click(object sender, RoutedEventArgs e)
        {
            var applicant = (sender as Button)?.Tag as Applicants;
            if (applicant == null) return;

            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить заявление",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = $"Заявление_{applicant.LastName}_{applicant.FirstName}_{DateTime.Now:yyyyMMdd}.docx"
            };
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    WordDocumentGenerator.GenerateApplication(applicant.Id, saveDialog.FileName);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при формировании заявления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GenerateConsent_Click(object sender, RoutedEventArgs e)
        {
            var applicant = (sender as Button)?.Tag as Applicants;
            if (applicant == null) return;

            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить согласие на обработку ПД",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = $"Согласие_{applicant.LastName}_{applicant.FirstName}_{DateTime.Now:yyyyMMdd}.docx"
            };
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    WordDocumentGenerator.GenerateConsent(applicant.Id, saveDialog.FileName);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при формировании согласия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GenerateGuardianConsent_Click(object sender, RoutedEventArgs e)
        {
            var applicant = (sender as Button)?.Tag as Applicants;
            if (applicant == null) return;

            var saveDialog = new SaveFileDialog
            {
                Title = "Сохранить согласие законного представителя",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = $"Согласие_представителя_{applicant.LastName}_{applicant.FirstName}_{DateTime.Now:yyyyMMdd}.docx"
            };
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    WordDocumentGenerator.GenerateGuardianConsent(applicant.Id, saveDialog.FileName);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при формировании согласия представителя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void NewApplicantButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SessionManager.Clear();
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                    mainWindow.MainFrame.Navigate(new ApplicantWizardPage());
                else
                    NavigationService?.Navigate(new ApplicantWizardPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании нового абитуриента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}