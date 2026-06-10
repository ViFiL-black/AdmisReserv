using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class IdentityPage : Page
    {
        private Applicants currentApplicant;
        private IdentityDocuments currentIdentityDocument;
        private bool isNewApplicant = true;
        private bool isLoadingData = false;

        public IdentityPage()
        {
            InitializeComponent();
            LoadReferenceData();

            // Начальная инициализация
            if (SessionManager.CurrentApplicant != null && SessionManager.CurrentApplicant.Id != 0)
            {
                currentApplicant = SessionManager.CurrentApplicant;
                isNewApplicant = false;
            }
            else
            {
                currentApplicant = new Applicants
                {
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                isNewApplicant = true;
            }

            // Основная загрузка данных при появлении страницы
            this.Loaded += IdentityPage_Loaded;
        }

        private void IdentityPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Если абитуриент уже существует – перезагружаем свежие данные из БД
            if (!isNewApplicant && currentApplicant.Id != 0)
            {
                var fresh = DataService.GetApplicant(currentApplicant.Id);
                if (fresh != null)
                {
                    currentApplicant = fresh;
                    SessionManager.CurrentApplicant = currentApplicant;
                }
            }

            LoadExistingIdentityDocuments();
            LoadApplicantData();
        }

        private void LoadReferenceData()
        {
            try
            {
                if (IdentityTypeCombo.ItemsSource == null || IdentityTypeCombo.Items.Count == 0)
                {
                    IdentityTypeCombo.ItemsSource = DataService.GetAll<IdentityDocumentTypes>();
                    IdentityTypeCombo.DisplayMemberPath = "Name";
                    IdentityTypeCombo.SelectedValuePath = "Id";
                }

                if (CitizenshipCombo.ItemsSource == null || CitizenshipCombo.Items.Count == 0)
                {
                    CitizenshipCombo.ItemsSource = DataService.GetAll<Citizenships>();
                    CitizenshipCombo.DisplayMemberPath = "Name";
                    CitizenshipCombo.SelectedValuePath = "Id";
                }

                if (CountryCombo.ItemsSource == null || CountryCombo.Items.Count == 0)
                {
                    CountryCombo.ItemsSource = DataService.GetByCondition<Countries>("IsActive = 1");
                    CountryCombo.DisplayMemberPath = "Name";
                    CountryCombo.SelectedValuePath = "Id";
                }

                if (GenderCombo.ItemsSource == null || GenderCombo.Items.Count == 0)
                {
                    GenderCombo.ItemsSource = DataService.GetAll<Genders>();
                    GenderCombo.DisplayMemberPath = "Name";
                    GenderCombo.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочных данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExistingIdentityDocuments()
        {
            if (currentApplicant == null || currentApplicant.Id == 0) return;

            try
            {
                var existingDocs = DataService.GetApplicantDocuments(currentApplicant.Id);

                if (existingDocs.Any())
                {
                    var docsWithDisplay = existingDocs.Select(d => new
                    {
                        d.Id,
                        d.DocumentTypeId,
                        d.Series,
                        d.Number,
                        d.IssuedBy,
                        d.IssueDate,
                        d.DepartmentCode,
                        d.IsPrimary,
                        d.ApplicantId,
                        d.AddedDate,
                        DisplayText = $"{GetDocumentTypeName(d.DocumentTypeId)} ({d.Series} {d.Number})"
                    }).ToList();

                    ExistingIdentityCombo.ItemsSource = docsWithDisplay;
                    ExistingIdentityCombo.DisplayMemberPath = "DisplayText";
                    ExistingIdentityCombo.SelectedValuePath = "Id";

                    ExistingIdentityRadio.IsChecked = true;
                    if (ExistingIdentityCombo.Items.Count > 0)
                        ExistingIdentityCombo.SelectedIndex = 0;
                    NewIdentityRadio.IsChecked = false;

                    LoadSelectedIdentityDocument();
                }
                else
                {
                    ExistingIdentityRadio.IsEnabled = false;
                    ExistingIdentityCombo.ItemsSource = null;
                    NewIdentityRadio.IsChecked = true;
                    ExistingIdentityRadio.IsChecked = false;
                    ClearDocumentFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearDocumentFields()
        {
            IdentityTypeCombo.SelectedIndex = -1;
            SeriesTextBox.Text = "";
            NumberTextBox.Text = "";
            IssuedByTextBox.Text = "";
            DepartmentCodeTextBox.Text = "";
            IssueDatePicker.SelectedDate = null;
        }

        private string GetDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return "Документ";
            try
            {
                var types = DataService.GetAll<IdentityDocumentTypes>();
                return types.FirstOrDefault(dt => dt.Id == documentTypeId.Value)?.Name ?? "Документ";
            }
            catch { return "Документ"; }
        }

        private void LoadApplicantData()
        {
            if (currentApplicant == null) return;
            isLoadingData = true;

            try
            {
                LastNameTextBox.Text = currentApplicant.LastName ?? "";
                FirstNameTextBox.Text = currentApplicant.FirstName ?? "";
                PatronymicTextBox.Text = currentApplicant.Patronymic ?? "";

                // Диагностика
                System.Diagnostics.Debug.WriteLine($"LoadApplicantData: BirthDate raw = {currentApplicant.BirthDate}, HasValue = {currentApplicant.BirthDate.HasValue}");

                // Установка даты рождения
                if (currentApplicant.BirthDate.HasValue && currentApplicant.BirthDate.Value > DateTime.MinValue)
                    BirthDatePicker.SelectedDate = currentApplicant.BirthDate.Value;
                else
                    BirthDatePicker.SelectedDate = null;

                BirthPlaceTextBox.Text = currentApplicant.BirthPlace ?? "";

                SetComboBoxValue(GenderCombo, currentApplicant.GenderId);
                SetComboBoxValue(CitizenshipCombo, currentApplicant.CitizenshipId);
                SetComboBoxValue(CountryCombo, currentApplicant.RegistrationCountryId);

                PostalCodeTextBox.Text = currentApplicant.RegistrationPostalCode ?? "";
                RegionTextBox.Text = currentApplicant.RegistrationRegion ?? "";
                DistrictTextBox.Text = currentApplicant.RegistrationDistrict ?? "";
                CityTextBox.Text = currentApplicant.RegistrationCity ?? "";
                StreetTextBox.Text = currentApplicant.RegistrationStreet ?? "";
                HouseTextBox.Text = currentApplicant.RegistrationHouse ?? "";
                BuildingTextBox.Text = currentApplicant.RegistrationBuilding ?? "";
                ApartmentTextBox.Text = currentApplicant.RegistrationApartment ?? "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadApplicantData error: {ex.Message}");
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private void LoadSelectedIdentityDocument()
        {
            if (ExistingIdentityCombo.SelectedItem == null) return;
            isLoadingData = true;

            try
            {
                var selectedId = (int)ExistingIdentityCombo.SelectedValue;
                currentIdentityDocument = DataService.GetDocument(selectedId);
                if (currentIdentityDocument == null) return;

                IdentityTypeCombo.SelectedValue = currentIdentityDocument.DocumentTypeId;
                SeriesTextBox.Text = currentIdentityDocument.Series ?? "";
                NumberTextBox.Text = currentIdentityDocument.Number ?? "";
                IssuedByTextBox.Text = currentIdentityDocument.IssuedBy ?? "";
                DepartmentCodeTextBox.Text = currentIdentityDocument.DepartmentCode ?? "";
                IssueDatePicker.SelectedDate = currentIdentityDocument.IssueDate;
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private bool SaveData()
        {
            try
            {
                if (!ValidateData()) return false;

                if (currentApplicant.Id == 0)
                {
                    currentApplicant.Id = DataService.CreateApplicant(currentApplicant);
                    SessionManager.CurrentApplicant = currentApplicant;
                    isNewApplicant = false;
                }

                currentApplicant.LastName = LastNameTextBox.Text.Trim();
                currentApplicant.FirstName = FirstNameTextBox.Text.Trim();
                currentApplicant.Patronymic = PatronymicTextBox.Text?.Trim();
                currentApplicant.BirthPlace = BirthPlaceTextBox.Text?.Trim();
                currentApplicant.BirthDate = BirthDatePicker.SelectedDate;
                currentApplicant.GenderId = GetComboBoxIntValue(GenderCombo);
                currentApplicant.CitizenshipId = GetComboBoxIntValue(CitizenshipCombo);
                currentApplicant.RegistrationCountryId = GetComboBoxIntValue(CountryCombo);
                currentApplicant.RegistrationPostalCode = PostalCodeTextBox.Text?.Trim();
                currentApplicant.RegistrationRegion = RegionTextBox.Text?.Trim();
                currentApplicant.RegistrationDistrict = DistrictTextBox.Text?.Trim();
                currentApplicant.RegistrationCity = CityTextBox.Text?.Trim();
                currentApplicant.RegistrationStreet = StreetTextBox.Text?.Trim();
                currentApplicant.RegistrationHouse = HouseTextBox.Text?.Trim();
                currentApplicant.RegistrationBuilding = BuildingTextBox.Text?.Trim();
                currentApplicant.RegistrationApartment = ApartmentTextBox.Text?.Trim();
                currentApplicant.UpdatedAt = DateTime.Now;

                DataService.UpdateApplicant(currentApplicant);

                if (ExistingIdentityRadio.IsChecked == true && currentIdentityDocument != null)
                    UpdateIdentityDocument(currentIdentityDocument);
                else
                    CreateNewIdentityDocument();

                SessionManager.CurrentApplicant = currentApplicant;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private int? GetComboBoxIntValue(ComboBox combo)
        {
            if (combo.SelectedValue != null && int.TryParse(combo.SelectedValue.ToString(), out int val))
                return val;
            return null;
        }

        private void UpdateIdentityDocument(IdentityDocuments doc)
        {
            doc.DocumentTypeId = GetComboBoxIntValue(IdentityTypeCombo);
            doc.Series = SeriesTextBox.Text?.Trim();
            doc.Number = NumberTextBox.Text?.Trim();
            doc.IssuedBy = IssuedByTextBox.Text?.Trim();
            doc.DepartmentCode = DepartmentCodeTextBox.Text?.Trim();
            doc.IssueDate = IssueDatePicker.SelectedDate;

            DataService.UpdateIdentityDocument(doc);
            DataService.LogChange("IdentityDocuments", doc.Id, "UPDATE");
        }

        private void CreateNewIdentityDocument()
        {
            var newDoc = new IdentityDocuments
            {
                ApplicantId = currentApplicant.Id,
                IsPrimary = true,
                AddedDate = DateTime.Now,
                DocumentTypeId = GetComboBoxIntValue(IdentityTypeCombo),
                Series = SeriesTextBox.Text?.Trim(),
                Number = NumberTextBox.Text?.Trim(),
                IssuedBy = IssuedByTextBox.Text?.Trim(),
                DepartmentCode = DepartmentCodeTextBox.Text?.Trim(),
                IssueDate = IssueDatePicker.SelectedDate
            };
            newDoc.Id = DataService.CreateIdentityDocument(newDoc);
            DataService.LogChange("IdentityDocuments", newDoc.Id, "INSERT");
        }

        private void ExistingIdentityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingData) return;
            if (ExistingIdentityRadio?.IsChecked == true)
                LoadSelectedIdentityDocument();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
                NavigationService?.Navigate(new ContactsPage());
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить ввод данных?\nВсе данные будут удалены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (SessionManager.CurrentApplicantId.HasValue)
                    DataService.DeleteApplicant(SessionManager.CurrentApplicantId.Value);
                SessionManager.Clear();
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    Application.Current.Shutdown();
            }
        }

        private void CheckAddressButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Проверка адреса будет реализована через интеграцию с ФИАС",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool ValidateData()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                errors.Add("• Фамилия обязательна для заполнения");
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                errors.Add("• Имя обязательно для заполнения");

            if (BirthDatePicker.SelectedDate.HasValue)
            {
                var birthDate = BirthDatePicker.SelectedDate.Value;
                var today = DateTime.Today;

                if (birthDate > today)
                    errors.Add("• Дата рождения не может быть в будущем");
                else
                {
                    int age = today.Year - birthDate.Year;
                    if (birthDate > today.AddYears(-age)) age--;

                    if (age < 14)
                        errors.Add("• Абитуриент должен быть не младше 14 лет");
                    else if (age > 120)
                        errors.Add("• Указан некорректный возраст (более 120 лет)");
                }
            }
            else
            {
                errors.Add("• Дата рождения обязательна для заполнения");
            }

            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
                errors.Add("• Номер документа обязателен для заполнения");
            if (!IssueDatePicker.SelectedDate.HasValue)
                errors.Add("• Дата выдачи документа обязательна для заполнения");
            if (string.IsNullOrWhiteSpace(CityTextBox.Text))
                errors.Add("• Населенный пункт обязателен для заполнения");
            if (StreetTextBox.Text.Contains("*") || StreetTextBox.Text.Contains("?"))
                errors.Add("• Улица не должна содержать символы подстановки * или ?");
            if (IdentityTypeCombo.SelectedValue == null)
                errors.Add("• Тип удостоверения личности обязателен для выбора");
            if (CitizenshipCombo.SelectedValue == null)
                errors.Add("• Гражданство обязательно для выбора");
            if (CountryCombo.SelectedValue == null)
                errors.Add("• Страна регистрации обязательна для выбора");

            if (errors.Any())
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки:\n\n" + string.Join("\n", errors),
                    "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void SetComboBoxValue(ComboBox comboBox, int? value)
        {
            if (value == null || value == 0)
            {
                comboBox.SelectedIndex = 0;
                return;
            }

            try
            {
                var items = comboBox.ItemsSource as System.Collections.IEnumerable;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var prop = item.GetType().GetProperty(comboBox.SelectedValuePath);
                        if (prop != null && prop.GetValue(item)?.ToString() == value.ToString())
                        {
                            comboBox.SelectedItem = item;
                            return;
                        }
                    }
                }
                comboBox.SelectedValue = value;
            }
            catch
            {
                comboBox.SelectedIndex = 0;
            }
        }
    }
}