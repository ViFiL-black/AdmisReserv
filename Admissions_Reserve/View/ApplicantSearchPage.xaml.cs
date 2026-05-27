using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

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
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                // Получаем значения из полей поиска
                string lastName = LastNameSearchBox.Text?.Trim() ?? "";
                string firstName = FirstNameSearchBox.Text?.Trim() ?? "";
                string patronymic = PatronymicSearchBox.Text?.Trim() ?? "";

                // Проверяем, что введено хотя бы одно поле
                if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(patronymic))
                {
                    MessageBox.Show("Пожалуйста, введите хотя бы одно поле для поиска (Фамилию, Имя или Отчество)",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SearchStatusText.Text = "Введите ФИО для поиска";
                    searchResults.Clear();
                    return;
                }

                // Получаем всех абитуриентов
                var allApplicants = DataService.GetAll<Applicants>();

                // Выполняем поиск с использованием похожести строк
                var results = allApplicants.Where(a =>
                {
                    bool exactMatch = false;
                    bool partialMatch = false;

                    // Проверяем фамилию
                    if (!string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(a.LastName))
                    {
                        if (a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.LastName.IndexOf(lastName, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false; // Если есть поле фамилии и оно не совпадает - исключаем
                    }

                    // Проверяем имя
                    if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(a.FirstName))
                    {
                        if (a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.FirstName.IndexOf(firstName, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false; // Если есть поле имени и оно не совпадает - исключаем
                    }

                    // Проверяем отчество
                    if (!string.IsNullOrWhiteSpace(patronymic) && !string.IsNullOrWhiteSpace(a.Patronymic))
                    {
                        if (a.Patronymic.Equals(patronymic, StringComparison.OrdinalIgnoreCase))
                            exactMatch = true;
                        else if (a.Patronymic.IndexOf(patronymic, StringComparison.OrdinalIgnoreCase) >= 0)
                            partialMatch = true;
                        else
                            return false; // Если есть поле отчества и оно не совпадает - исключаем
                    }

                    return exactMatch || partialMatch;
                }).OrderBy(a => a.LastName).ThenBy(a => a.FirstName).ToList();

                // Обновляем результаты поиска
                searchResults.Clear();
                foreach (var applicant in results)
                {
                    searchResults.Add(applicant);
                }

                // Обновляем статус
                if (results.Count == 0)
                {
                    SearchStatusText.Text = "Похожих абитуриентов не найдено";
                }
                else if (results.Count == 1)
                {
                    SearchStatusText.Text = "Найден 1 абитуриент";
                }
                else
                {
                    SearchStatusText.Text = $"Найдено {results.Count} абитуриентов";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SearchStatusText.Text = "Ошибка при поиске";
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            LastNameSearchBox.Text = "";
            FirstNameSearchBox.Text = "";
            PatronymicSearchBox.Text = "";
            searchResults.Clear();
            SearchStatusText.Text = "Введите ФИО для поиска";
            LastNameSearchBox.Focus();
        }

        private void SearchResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedApplicant = SearchResultsGrid.SelectedItem as Applicants;
        }

        private void SearchResultsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedApplicant != null)
            {
                OpenApplicant(selectedApplicant, editMode: false);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicant = button?.Tag as Applicants;

            if (applicant != null)
            {
                OpenApplicant(applicant, editMode: false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicant = button?.Tag as Applicants;

            if (applicant != null)
            {
                OpenApplicant(applicant, editMode: true);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicant = button?.Tag as Applicants;

            if (applicant != null)
            {
                DeleteApplicant(applicant);
            }
        }

        private void OpenApplicant(Applicants applicant, bool editMode)
        {
            try
            {
                // Загружаем абитуриента в SessionManager
                SessionManager.CurrentApplicant = applicant;

                // Если режим редактирования, открываем через мастер с навигацией
                if (editMode)
                {
                    var wizardPage = new ApplicantWizardPage();
                    NavigationService?.Navigate(wizardPage);
                }
                else
                {
                    // В режиме просмотра открываем первую страницу
                    var identityPage = new IdentityPage();
                    NavigationService?.Navigate(identityPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии профиля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteApplicant(Applicants applicant)
        {
            try
            {
                // Запрашиваем подтверждение удаления
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить абитуриента {applicant.LastName} {applicant.FirstName}? " +
                    $"\r\nЭто действие необратимо и удалит все связанные данные.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Выполняем удаление
                    DataService.DeleteApplicant(applicant.Id);

                    // Удаляем из списка результатов
                    searchResults.Remove(applicant);

                    // Очищаем выбор
                    selectedApplicant = null;

                    // Показываем сообщение об успешном удалении
                    MessageBox.Show($"Абитуриент {applicant.LastName} {applicant.FirstName} успешно удален",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем статус
                    if (searchResults.Count == 0)
                    {
                        SearchStatusText.Text = "Результаты удалены. Похожих абитуриентов не найдено";
                    }
                    else if (searchResults.Count == 1)
                    {
                        SearchStatusText.Text = "Найден 1 абитуриент";
                    }
                    else
                    {
                        SearchStatusText.Text = $"Найдено {searchResults.Count} абитуриентов";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении абитуриента: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void NewApplicantButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Очищаем SessionManager для создания нового абитуриента
                SessionManager.Clear();
                
                // Переходим в мастер добавления (сверху отображаются шаги)
                var wizardPage = new ApplicantWizardPage();
                NavigationService?.Navigate(wizardPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании нового абитуриента: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
