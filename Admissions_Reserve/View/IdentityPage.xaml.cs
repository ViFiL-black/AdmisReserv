// IdentityPage.xaml.cs
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
        private bool isInitialized = false;

        public IdentityPage()
        {
            InitializeComponent();
            LoadReferenceData();

            if (SessionManager.CurrentApplicant != null && SessionManager.CurrentApplicant.Id != 0)
            {
                currentApplicant = SessionManager.CurrentApplicant;
                isNewApplicant = false;
                LoadExistingIdentityDocuments();
                LoadApplicantData();
            }
            else
            {
                // Абитуриент будет создан только при сохранении данных (нажатии кнопки "Далее")
                currentApplicant = new Applicants
                {
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                isNewApplicant = true;
            }
            isInitialized = true;
        }

        private void LoadReferenceData()
        {
            try
            {
                var identityTypes = DataService.GetAll<IdentityDocumentTypes>();
                IdentityTypeCombo.ItemsSource = identityTypes;
                IdentityTypeCombo.DisplayMemberPath = "Name";
                IdentityTypeCombo.SelectedValuePath = "Id";

                var citizenships = DataService.GetAll<Citizenships>();
                CitizenshipCombo.ItemsSource = citizenships;
                CitizenshipCombo.DisplayMemberPath = "Name";
                CitizenshipCombo.SelectedValuePath = "Id";

                ActualCountryCombo.ItemsSource = citizenships;
                ActualCountryCombo.DisplayMemberPath = "Name";
                ActualCountryCombo.SelectedValuePath = "Id";

                var countries = DataService.GetByCondition<Countries>("IsActive = 1");
                CountryCombo.ItemsSource = countries;
                CountryCombo.DisplayMemberPath = "Name";
                CountryCombo.SelectedValuePath = "Id";

                var genders = DataService.GetAll<Genders>();
                GenderCombo.ItemsSource = genders;
                GenderCombo.DisplayMemberPath = "Name";
                GenderCombo.SelectedValuePath = "Id";
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

                    ExistingIdentityCombo.ItemsSource = null;
                    ExistingIdentityCombo.ItemsSource = docsWithDisplay;
                    ExistingIdentityCombo.DisplayMemberPath = "DisplayText";
                    ExistingIdentityCombo.SelectedValuePath = "Id";

                    ExistingIdentityRadio.IsChecked = true;
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
                var types = DataService.GetAll<IdentityDocumentTypes>();
                var docType = types.FirstOrDefault(dt => dt.Id == documentTypeId.Value);
                return docType?.Name ?? "Документ";
            }
            catch
            {
                return "Документ";
            }
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

                if (currentApplicant.BirthDate != null && currentApplicant.BirthDate != default(DateTime))
                    BirthDatePicker.SelectedDate = currentApplicant.BirthDate;

                BirthPlaceTextBox.Text = currentApplicant.BirthPlace ?? "";

                if (currentApplicant.GenderId.HasValue)
                    GenderCombo.SelectedValue = currentApplicant.GenderId.Value;

                if (currentApplicant.CitizenshipId.HasValue)
                    CitizenshipCombo.SelectedValue = currentApplicant.CitizenshipId.Value;

                if (!string.IsNullOrEmpty(currentApplicant.Snils))
                {
                    SnilsTextBox.Text = FormatSnils(currentApplicant.Snils);
                }

                if (currentApplicant.RegistrationCountryId.HasValue)
                    CountryCombo.SelectedValue = currentApplicant.RegistrationCountryId.Value;

                PostalCodeTextBox.Text = currentApplicant.RegistrationPostalCode ?? "";
                RegionTextBox.Text = currentApplicant.RegistrationRegion ?? "";
                DistrictTextBox.Text = currentApplicant.RegistrationDistrict ?? "";
                CityTextBox.Text = currentApplicant.RegistrationCity ?? "";
                StreetTextBox.Text = currentApplicant.RegistrationStreet ?? "";
                HouseTextBox.Text = currentApplicant.RegistrationHouse ?? "";
                BuildingTextBox.Text = currentApplicant.RegistrationBuilding ?? "";
                ApartmentTextBox.Text = currentApplicant.RegistrationApartment ?? "";

                if (currentApplicant.ActualAddressSame == false)
                {
                    SameAsRegistrationCheckBox.IsChecked = false;
                    ActualAddressBorder.Visibility = Visibility.Visible;

                    if (currentApplicant.ActualCountryId.HasValue)
                        ActualCountryCombo.SelectedValue = currentApplicant.ActualCountryId.Value;

                    ActualPostalCodeTextBox.Text = currentApplicant.ActualPostalCode ?? "";
                    ActualRegionTextBox.Text = currentApplicant.ActualRegion ?? "";
                    ActualCityTextBox.Text = currentApplicant.ActualCity ?? "";
                    ActualStreetTextBox.Text = currentApplicant.ActualStreet ?? "";
                    ActualHouseTextBox.Text = currentApplicant.ActualHouse ?? "";
                    ActualApartmentTextBox.Text = currentApplicant.ActualApartment ?? "";
                }
                else
                {
                    SameAsRegistrationCheckBox.IsChecked = true;
                    ActualAddressBorder.Visibility = Visibility.Collapsed;
                }
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private void LoadSelectedIdentityDocument()
        {
            if (ExistingIdentityCombo.SelectedItem == null) return;

            var selectedId = (int)ExistingIdentityCombo.SelectedValue;
            currentIdentityDocument = DataService.GetDocument(selectedId);

            if (currentIdentityDocument == null) return;

            isLoadingData = true;

            try
            {
                IdentityTypeCombo.SelectedValue = currentIdentityDocument.DocumentTypeId;
                SeriesTextBox.Text = currentIdentityDocument.Series ?? "";
                NumberTextBox.Text = currentIdentityDocument.Number ?? "";
                IssuedByTextBox.Text = currentIdentityDocument.IssuedBy ?? "";
                DepartmentCodeTextBox.Text = currentIdentityDocument.DepartmentCode ?? "";

                if (currentIdentityDocument.IssueDate.HasValue)
                    IssueDatePicker.SelectedDate = currentIdentityDocument.IssueDate.Value;
                else
                    IssueDatePicker.SelectedDate = null;
            }
            finally
            {
                isLoadingData = false;
            }
        }

        private string FormatSnils(string snils)
        {
            if (string.IsNullOrEmpty(snils) || snils.Length != 11)
                return snils;

            return $"{snils.Substring(0, 3)}-{snils.Substring(3, 3)}-{snils.Substring(6, 3)} {snils.Substring(9, 2)}";
        }

        private bool SaveData()
        {
            try
            {
                if (!ValidateData())
                    return false;

                // Если абитуриент еще не создан (нет ID), создаем его в БД
                if (currentApplicant.Id == 0)
                {
                    currentApplicant.Id = DataService.CreateApplicant(currentApplicant);
                    SessionManager.CurrentApplicant = currentApplicant;
                }

                // Обновляем данные из формы
                currentApplicant.LastName = LastNameTextBox.Text.Trim();
                currentApplicant.FirstName = FirstNameTextBox.Text.Trim();
                currentApplicant.Patronymic = PatronymicTextBox.Text?.Trim();
                currentApplicant.BirthPlace = BirthPlaceTextBox.Text?.Trim();

                if (BirthDatePicker.SelectedDate.HasValue)
                    currentApplicant.BirthDate = BirthDatePicker.SelectedDate.Value;

                if (GenderCombo.SelectedValue != null)
                    currentApplicant.GenderId = (int)GenderCombo.SelectedValue;

                if (CitizenshipCombo.SelectedValue != null)
                    currentApplicant.CitizenshipId = (int)CitizenshipCombo.SelectedValue;

                if (!string.IsNullOrWhiteSpace(SnilsTextBox.Text) && NoSnilsCheckBox.IsChecked != true)
                {
                    currentApplicant.Snils = SnilsTextBox.Text.Replace("-", "").Replace(" ", "").Trim();
                }
                else if (NoSnilsCheckBox.IsChecked == true)
                {
                    currentApplicant.Snils = null;
                }

                if (CountryCombo.SelectedValue != null)
                    currentApplicant.RegistrationCountryId = (int)CountryCombo.SelectedValue;

                currentApplicant.RegistrationPostalCode = PostalCodeTextBox.Text?.Trim();
                currentApplicant.RegistrationRegion = RegionTextBox.Text?.Trim();
                currentApplicant.RegistrationDistrict = DistrictTextBox.Text?.Trim();
                currentApplicant.RegistrationCity = CityTextBox.Text?.Trim();
                currentApplicant.RegistrationStreet = StreetTextBox.Text?.Trim();
                currentApplicant.RegistrationHouse = HouseTextBox.Text?.Trim();
                currentApplicant.RegistrationBuilding = BuildingTextBox.Text?.Trim();
                currentApplicant.RegistrationApartment = ApartmentTextBox.Text?.Trim();

                currentApplicant.ActualAddressSame = SameAsRegistrationCheckBox.IsChecked ?? true;

                if ((bool)!currentApplicant.ActualAddressSame)
                {
                    if (ActualCountryCombo.SelectedValue != null)
                        currentApplicant.ActualCountryId = (int)ActualCountryCombo.SelectedValue;

                    currentApplicant.ActualPostalCode = ActualPostalCodeTextBox.Text?.Trim();
                    currentApplicant.ActualRegion = ActualRegionTextBox.Text?.Trim();
                    currentApplicant.ActualCity = ActualCityTextBox.Text?.Trim();
                    currentApplicant.ActualStreet = ActualStreetTextBox.Text?.Trim();
                    currentApplicant.ActualHouse = ActualHouseTextBox.Text?.Trim();
                    currentApplicant.ActualApartment = ActualApartmentTextBox.Text?.Trim();
                }
                else
                {
                    currentApplicant.ActualCountryId = null;
                    currentApplicant.ActualPostalCode = null;
                    currentApplicant.ActualRegion = null;
                    currentApplicant.ActualCity = null;
                    currentApplicant.ActualStreet = null;
                    currentApplicant.ActualHouse = null;
                    currentApplicant.ActualApartment = null;
                }

                currentApplicant.UpdatedAt = DateTime.Now;

                // Сохраняем в БД
                DataService.UpdateApplicant(currentApplicant);

                // Сохраняем документ
                if (ExistingIdentityRadio.IsChecked == true && currentIdentityDocument != null)
                {
                    UpdateIdentityDocument(currentIdentityDocument);
                }
                else
                {
                    CreateNewIdentityDocument();
                }

                // Обновляем сессию
                SessionManager.CurrentApplicant = currentApplicant;

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void UpdateIdentityDocument(IdentityDocuments doc)
        {
            if (IdentityTypeCombo.SelectedValue != null)
                doc.DocumentTypeId = (int)IdentityTypeCombo.SelectedValue;

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
                AddedDate = DateTime.Now
            };

            if (IdentityTypeCombo.SelectedValue != null)
                newDoc.DocumentTypeId = (int)IdentityTypeCombo.SelectedValue;

            newDoc.Series = SeriesTextBox.Text?.Trim();
            newDoc.Number = NumberTextBox.Text?.Trim();
            newDoc.IssuedBy = IssuedByTextBox.Text?.Trim();
            newDoc.DepartmentCode = DepartmentCodeTextBox.Text?.Trim();
            newDoc.IssueDate = IssueDatePicker.SelectedDate;

            newDoc.Id = DataService.CreateIdentityDocument(newDoc);
            DataService.LogChange("IdentityDocuments", newDoc.Id, "INSERT");
        }

        private void ExistingIdentityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized || isLoadingData) return;

            if (ExistingIdentityRadio != null && ExistingIdentityRadio.IsChecked == true)
            {
                LoadSelectedIdentityDocument();
            }
        }

        private void SameAddressCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            ActualAddressBorder.Visibility = Visibility.Collapsed;
        }

        private void SameAddressCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            ActualAddressBorder.Visibility = Visibility.Visible;
        }

        private void NoSnilsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            SnilsTextBox.IsEnabled = false;
            SnilsTextBox.Text = string.Empty;
        }

        private void NoSnilsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            SnilsTextBox.IsEnabled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
            {
                NavigationService?.Navigate(new ContactsPage());
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();

            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить ввод данных?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (SessionManager.CurrentApplicantId.HasValue)
                {
                    DataService.DeleteApplicant(SessionManager.CurrentApplicantId.Value);
                    DataService.LogChange("Applicants", SessionManager.CurrentApplicantId.Value, "DELETE");
                }

                SessionManager.Clear();

                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    Application.Current.Windows[0]?.Close();
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

            if (!BirthDatePicker.SelectedDate.HasValue)
                errors.Add("• Дата рождения обязательна для заполнения");

            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
                errors.Add("• Номер документа обязателен для заполнения");

            if (!IssueDatePicker.SelectedDate.HasValue)
                errors.Add("• Дата выдачи документа обязательна для заполнения");

            if (string.IsNullOrWhiteSpace(CityTextBox.Text))
                errors.Add("• Населенный пункт обязателен для заполнения");

            if (IdentityTypeCombo.SelectedValue == null)
                errors.Add("• Тип удостоверения личности обязателен для выбора");

            if (CitizenshipCombo.SelectedValue == null)
                errors.Add("• Гражданство обязательно для выбора");

            if (CountryCombo.SelectedValue == null)
                errors.Add("• Страна регистрации обязательна для выбора");

            if (errors.Any())
            {
                MessageBox.Show("Пожалуйста, исправьте следующие ошибки:\n\n" + string.Join("\n", errors),
                    "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}