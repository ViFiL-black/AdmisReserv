using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class ApplicationTypeAndEducationPage : Page
    {
        private EducationDocuments currentEducationDocument;
        private bool isLoadingData = false;
        private bool isInitialized = false;
        private bool isNavigating = false;

        // Класс для хранения данных формы
        private class EducationFormData
        {
            public int? ApplicationTypeId { get; set; }
            public bool FirstTimeEducation { get; set; }
            public int? CountryId { get; set; }
            public string City { get; set; }
            public string EducationalOrg { get; set; }
            public int? DocumentTypeId { get; set; }
            public int? EducationLevelId { get; set; }
            public int? DocumentEducationLevelId { get; set; }
            public string Series { get; set; }
            public string Number { get; set; }
            public DateTime? IssueDate { get; set; }
            public DateTime? GraduationYear { get; set; }
            public int SatisfactoryCount { get; set; }
            public int GoodCount { get; set; }
            public int ExcellentCount { get; set; }
            public double AverageScore { get; set; }
            public bool FrdoVerified { get; set; }
            public string ScanFilePath { get; set; }
            public int? DocumentFormId { get; set; }
            public string OriginalOrganization { get; set; }
        }

        public ApplicationTypeAndEducationPage()
        {
            InitializeComponent();
            Loaded += ApplicationTypeAndEducationPage_Loaded;
        }

        private void ApplicationTypeAndEducationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReferenceData();
            LoadExistingDocuments();
            isInitialized = true;
        }

        private void LoadReferenceData()
        {
            try
            {
                var applicationTypes = DataService.GetAll<ApplicationTypes>();
                if (applicationTypes.Any())
                {
                    ApplicationTypeCombo.ItemsSource = applicationTypes;
                    ApplicationTypeCombo.DisplayMemberPath = "Name";
                    ApplicationTypeCombo.SelectedValuePath = "Id";
                    if (ApplicationTypeCombo.Items.Count > 0)
                        ApplicationTypeCombo.SelectedIndex = 0;
                }

                var countries = DataService.GetByCondition<Countries>("IsActive = 1");
                if (countries.Any())
                {
                    CountryCombo.ItemsSource = countries;
                    CountryCombo.DisplayMemberPath = "Name";
                    CountryCombo.SelectedValuePath = "Id";
                    if (CountryCombo.Items.Count > 0)
                        CountryCombo.SelectedIndex = 0;
                }

                var docTypes = DataService.GetAll<EducationDocumentTypes>();
                if (docTypes.Any())
                {
                    DocumentTypeCombo.ItemsSource = docTypes;
                    DocumentTypeCombo.DisplayMemberPath = "Name";
                    DocumentTypeCombo.SelectedValuePath = "Id";
                    if (DocumentTypeCombo.Items.Count > 0)
                        DocumentTypeCombo.SelectedIndex = 0;
                }

                var educationLevels = DataService.GetAll<EducationLevels>();
                if (educationLevels.Any())
                {
                    EducationLevelCombo.ItemsSource = educationLevels;
                    EducationLevelCombo.DisplayMemberPath = "Name";
                    EducationLevelCombo.SelectedValuePath = "Id";
                    if (EducationLevelCombo.Items.Count > 0)
                        EducationLevelCombo.SelectedIndex = 0;

                    DocumentEducationLevelCombo.ItemsSource = educationLevels;
                    DocumentEducationLevelCombo.DisplayMemberPath = "Name";
                    DocumentEducationLevelCombo.SelectedValuePath = "Id";
                    if (DocumentEducationLevelCombo.Items.Count > 0)
                        DocumentEducationLevelCombo.SelectedIndex = 0;
                }

                var docForms = DataService.GetAll<DocumentForms>();
                if (docForms.Any())
                {
                    DocumentFormCombo.ItemsSource = docForms;
                    DocumentFormCombo.DisplayMemberPath = "Name";
                    DocumentFormCombo.SelectedValuePath = "Id";
                    if (DocumentFormCombo.Items.Count > 0)
                        DocumentFormCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочных данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExistingDocuments()
        {
            try
            {
                if (SessionManager.CurrentApplicantId == null)
                {
                    ExistingDocumentRadio.IsEnabled = false;
                    ExistingDocumentsCombo.ItemsSource = null;
                    NewDocumentRadio.IsChecked = true;
                    ExistingDocumentRadio.IsChecked = false;
                    return;
                }

                var existingDocs = DataService.GetApplicantEducationDocuments(SessionManager.CurrentApplicantId.Value);

                if (existingDocs.Any())
                {
                    var docsWithDisplay = existingDocs.Select(d => new
                    {
                        d.Id,
                        DisplayText = $"{GetDocumentTypeName(d.DocumentTypeId)} №{d.Number} - {d.EducationalOrg}"
                    }).ToList();

                    ExistingDocumentsCombo.ItemsSource = docsWithDisplay;
                    ExistingDocumentsCombo.DisplayMemberPath = "DisplayText";
                    ExistingDocumentsCombo.SelectedValuePath = "Id";

                    if (ExistingDocumentsCombo.Items.Count > 0)
                    {
                        ExistingDocumentsCombo.SelectedIndex = 0;
                        ExistingDocumentRadio.IsChecked = true;
                        NewDocumentRadio.IsChecked = false;

                        var selectedId = (int)ExistingDocumentsCombo.SelectedValue;
                        currentEducationDocument = DataService.GetEducationDocument(selectedId);
                        LoadDocumentData();
                    }
                }
                else
                {
                    ExistingDocumentRadio.IsEnabled = false;
                    ExistingDocumentsCombo.ItemsSource = null;
                    NewDocumentRadio.IsChecked = true;
                    ExistingDocumentRadio.IsChecked = false;
                    currentEducationDocument = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return "Документ";
            try
            {
                var types = DataService.GetAll<EducationDocumentTypes>();
                var docType = types.FirstOrDefault(dt => dt.Id == documentTypeId.Value);
                return docType?.Name ?? "Документ";
            }
            catch { return "Документ"; }
        }

        private void LoadDocumentData()
        {
            if (currentEducationDocument == null) return;

            isLoadingData = true;

            try
            {
                if (currentEducationDocument.ApplicationTypeId.HasValue)
                    ApplicationTypeCombo.SelectedValue = currentEducationDocument.ApplicationTypeId.Value;
                else if (ApplicationTypeCombo.Items.Count > 0)
                    ApplicationTypeCombo.SelectedIndex = 0;

                FirstTimeEducationCheckBox.IsChecked = currentEducationDocument.FirstTimeEducation;

                if (currentEducationDocument.CountryId.HasValue)
                    CountryCombo.SelectedValue = currentEducationDocument.CountryId.Value;
                else if (CountryCombo.Items.Count > 0)
                    CountryCombo.SelectedIndex = 0;

                CityTextBox.Text = currentEducationDocument.City ?? "";
                EducationalOrgTextBox.Text = currentEducationDocument.EducationalOrg ?? "";

                if (currentEducationDocument.DocumentTypeId.HasValue)
                    DocumentTypeCombo.SelectedValue = currentEducationDocument.DocumentTypeId.Value;
                else if (DocumentTypeCombo.Items.Count > 0)
                    DocumentTypeCombo.SelectedIndex = 0;

                if (currentEducationDocument.EducationLevelId.HasValue)
                    EducationLevelCombo.SelectedValue = currentEducationDocument.EducationLevelId.Value;
                else if (EducationLevelCombo.Items.Count > 0)
                    EducationLevelCombo.SelectedIndex = 0;

                if (currentEducationDocument.DocumentEducationLevelId.HasValue)
                    DocumentEducationLevelCombo.SelectedValue = currentEducationDocument.DocumentEducationLevelId.Value;
                else if (DocumentEducationLevelCombo.Items.Count > 0)
                    DocumentEducationLevelCombo.SelectedIndex = 0;

                SeriesTextBox.Text = currentEducationDocument.Series ?? "";
                NumberTextBox.Text = currentEducationDocument.Number ?? "";

                IssueDatePicker.SelectedDate = currentEducationDocument.IssueDate;
                GraduationYearPicker.SelectedDate = currentEducationDocument.GraduationYear;

                SatisfactoryCountTextBox.Text = currentEducationDocument.SatisfactoryCount.ToString();
                GoodCountTextBox.Text = currentEducationDocument.GoodCount.ToString();
                ExcellentCountTextBox.Text = currentEducationDocument.ExcellentCount.ToString();

                CalculateAverageScore();

                FrdoVerifiedCheckBox.IsChecked = currentEducationDocument.FrdoVerified;

                if (currentEducationDocument.DocumentFormId.HasValue)
                    DocumentFormCombo.SelectedValue = currentEducationDocument.DocumentFormId.Value;
                else if (DocumentFormCombo.Items.Count > 0)
                    DocumentFormCombo.SelectedIndex = 0;

                OriginalOrganizationTextBox.Text = currentEducationDocument.OriginalOrganization ?? "";
                ScanFileTextBox.Text = currentEducationDocument.ScanFilePath ?? "";
            }
            finally
            {
                isLoadingData = false;
            }
        }

        // Сбор данных из формы в UI потоке
        private EducationFormData CollectFormData()
        {
            var formData = new EducationFormData();

            // Сохраняем все значения из UI элементов
            if (ApplicationTypeCombo.SelectedValue != null)
                formData.ApplicationTypeId = (int)ApplicationTypeCombo.SelectedValue;

            formData.FirstTimeEducation = FirstTimeEducationCheckBox.IsChecked ?? false;

            if (CountryCombo.SelectedValue != null)
                formData.CountryId = (int)CountryCombo.SelectedValue;

            formData.City = CityTextBox.Text?.Trim();
            formData.EducationalOrg = EducationalOrgTextBox.Text?.Trim();

            if (DocumentTypeCombo.SelectedValue != null)
                formData.DocumentTypeId = (int)DocumentTypeCombo.SelectedValue;

            if (EducationLevelCombo.SelectedValue != null)
                formData.EducationLevelId = (int)EducationLevelCombo.SelectedValue;

            if (DocumentEducationLevelCombo.SelectedValue != null)
                formData.DocumentEducationLevelId = (int)DocumentEducationLevelCombo.SelectedValue;

            formData.Series = SeriesTextBox.Text?.Trim();
            formData.Number = NumberTextBox.Text?.Trim();
            formData.IssueDate = IssueDatePicker.SelectedDate;
            formData.GraduationYear = GraduationYearPicker.SelectedDate;

            formData.SatisfactoryCount = ParseInt(SatisfactoryCountTextBox.Text);
            formData.GoodCount = ParseInt(GoodCountTextBox.Text);
            formData.ExcellentCount = ParseInt(ExcellentCountTextBox.Text);

            if (double.TryParse(AverageScoreTextBox.Text, out double avgScore))
                formData.AverageScore = avgScore;

            formData.FrdoVerified = FrdoVerifiedCheckBox.IsChecked ?? false;
            formData.ScanFilePath = ScanFileTextBox.Text;

            if (DocumentFormCombo.SelectedValue != null)
                formData.DocumentFormId = (int)DocumentFormCombo.SelectedValue;

            formData.OriginalOrganization = OriginalOrganizationTextBox.Text?.Trim();

            return formData;
        }

        private void ExistingDocumentRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            if (ExistingDocumentsCombo != null)
            {
                ExistingDocumentsCombo.IsEnabled = true;

                if (ExistingDocumentsCombo.SelectedItem != null)
                {
                    var selectedId = (int)ExistingDocumentsCombo.SelectedValue;
                    currentEducationDocument = DataService.GetEducationDocument(selectedId);
                    LoadDocumentData();
                }
            }
        }

        private void NewDocumentRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            if (ExistingDocumentsCombo != null)
            {
                ExistingDocumentsCombo.IsEnabled = false;
            }

            ClearForm();
            currentEducationDocument = null;
        }

        private void ExistingDocumentsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized || isLoadingData) return;

            if (ExistingDocumentRadio != null && ExistingDocumentRadio.IsChecked == true
                && ExistingDocumentsCombo.SelectedItem != null)
            {
                var selectedId = (int)ExistingDocumentsCombo.SelectedValue;
                currentEducationDocument = DataService.GetEducationDocument(selectedId);
                LoadDocumentData();
            }
        }

        private void ClearForm()
        {
            if (ApplicationTypeCombo.Items.Count > 0)
                ApplicationTypeCombo.SelectedIndex = 0;

            FirstTimeEducationCheckBox.IsChecked = false;

            if (CountryCombo.Items.Count > 0)
                CountryCombo.SelectedIndex = 0;

            CityTextBox.Text = "";
            EducationalOrgTextBox.Text = "";

            if (DocumentTypeCombo.Items.Count > 0)
                DocumentTypeCombo.SelectedIndex = 0;

            if (EducationLevelCombo.Items.Count > 0)
                EducationLevelCombo.SelectedIndex = 0;

            if (DocumentEducationLevelCombo.Items.Count > 0)
                DocumentEducationLevelCombo.SelectedIndex = 0;

            SeriesTextBox.Text = "";
            NumberTextBox.Text = "";
            IssueDatePicker.SelectedDate = null;
            GraduationYearPicker.SelectedDate = null;
            SatisfactoryCountTextBox.Text = "0";
            GoodCountTextBox.Text = "0";
            ExcellentCountTextBox.Text = "0";
            AverageScoreTextBox.Text = "0.00";
            FrdoVerifiedCheckBox.IsChecked = false;

            if (DocumentFormCombo.Items.Count > 0)
                DocumentFormCombo.SelectedIndex = 0;

            OriginalOrganizationTextBox.Text = "";
            ScanFileTextBox.Text = "";
        }

        private void Grades_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
            CalculateAverageScore();
        }

        private void CalculateAverageScore()
        {
            int satisfactory = ParseInt(SatisfactoryCountTextBox.Text);
            int good = ParseInt(GoodCountTextBox.Text);
            int excellent = ParseInt(ExcellentCountTextBox.Text);

            int totalSubjects = satisfactory + good + excellent;
            if (totalSubjects > 0)
            {
                double average = (double)(satisfactory * 3 + good * 4 + excellent * 5) / totalSubjects;
                AverageScoreTextBox.Text = Math.Round(average, 2).ToString("0.00");
            }
            else
            {
                AverageScoreTextBox.Text = "0.00";
            }
        }

        private int ParseInt(string value)
        {
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }

        private void UploadScanButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл скан-копии документа об образовании",
                Filter = "PDF файлы (*.pdf)|*.pdf|Изображения (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ScanFileTextBox.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void CheckFrdoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SeriesTextBox.Text) && !string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                var result = MessageBox.Show($"Проверить документ {SeriesTextBox.Text} {NumberTextBox.Text} в ФРДО?",
                    "Проверка в ФРДО", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    FrdoVerifiedCheckBox.IsChecked = true;
                    MessageBox.Show("Документ успешно проверен в ФРДО", "Результат проверки",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните серию и номер документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Сохранение данных (выполняется в UI потоке, так как обращается к UI элементам)
        private bool SaveData()
        {
            try
            {
                if (!ValidateForm())
                    return false;

                if (SessionManager.CurrentApplicantId == null)
                {
                    MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Собираем данные из формы в UI потоке
                var formData = CollectFormData();

                // Работа с БД может быть в этом же потоке, так как SQLite операции быстрые
                if (currentEducationDocument == null)
                {
                    currentEducationDocument = new EducationDocuments
                    {
                        ApplicantId = SessionManager.CurrentApplicantId.Value,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                }

                // Заполняем данными из формы
                currentEducationDocument.ApplicationTypeId = formData.ApplicationTypeId;
                currentEducationDocument.FirstTimeEducation = formData.FirstTimeEducation;
                currentEducationDocument.CountryId = formData.CountryId;
                currentEducationDocument.City = formData.City;
                currentEducationDocument.EducationalOrg = formData.EducationalOrg;
                currentEducationDocument.DocumentTypeId = formData.DocumentTypeId;
                currentEducationDocument.EducationLevelId = formData.EducationLevelId;
                currentEducationDocument.DocumentEducationLevelId = formData.DocumentEducationLevelId;
                currentEducationDocument.Series = formData.Series;
                currentEducationDocument.Number = formData.Number;
                currentEducationDocument.IssueDate = formData.IssueDate;
                currentEducationDocument.GraduationYear = formData.GraduationYear;
                currentEducationDocument.SatisfactoryCount = formData.SatisfactoryCount;
                currentEducationDocument.GoodCount = formData.GoodCount;
                currentEducationDocument.ExcellentCount = formData.ExcellentCount;
                currentEducationDocument.AverageScore = formData.AverageScore;
                currentEducationDocument.FrdoVerified = formData.FrdoVerified;
                currentEducationDocument.ScanFilePath = formData.ScanFilePath;
                currentEducationDocument.DocumentFormId = formData.DocumentFormId;
                currentEducationDocument.OriginalOrganization = formData.OriginalOrganization;
                currentEducationDocument.UpdatedAt = DateTime.Now;

                // Сохраняем в БД
                if (currentEducationDocument.Id == 0)
                {
                    currentEducationDocument.Id = DataService.CreateEducationDocument(currentEducationDocument);
                    DataService.LogChange("EducationDocuments", currentEducationDocument.Id, "INSERT");
                }
                else
                {
                    DataService.UpdateEducationDocument(currentEducationDocument);
                    DataService.LogChange("EducationDocuments", currentEducationDocument.Id, "UPDATE");
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool ValidateForm()
        {
            if (ApplicationTypeCombo.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите вид заявления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ApplicationTypeCombo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(CityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите населенный пункт", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CityTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EducationalOrgTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите образовательную организацию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EducationalOrgTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите номер документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NumberTextBox.Focus();
                return false;
            }

            if (IssueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, укажите дату выдачи документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (GraduationYearPicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, укажите год окончания", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // В ApplicationTypeAndEducationPage.xaml.cs замените метод NextButton_Click на этот:

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNavigating) return;

            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            isNavigating = true;

            try
            {
                if (SaveData())
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    // Переход на страницу DocumentsPage
                    NavigationService?.Navigate(new DocumentsPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isNavigating = false;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }
        // Кнопка НАЗАД - возврат на страницу контактов
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNavigating) return;

            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            try
            {
                SaveData();

                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
            }
            finally
            {
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        // Кнопка ОТМЕНИТЬ
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNavigating) return;

            var result = MessageBox.Show("Вы уверены, что хотите отменить ввод данных?\nВсе несохраненные данные будут потеряны.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionManager.Clear();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainFrame.Navigate(new WelcomePage());
                }
                else if (NavigationService?.CanGoBack == true)
                {
                    while (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}