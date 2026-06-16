using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class AdditionalInfoPage : Page
    {
        private int? _currentApplicantId;
        private bool isInitialized = false;
        private bool isSaving = false;
        private bool _isUpdatingSnils = false; // флаг для предотвращения рекурсии при обновлении TextBox

        private ObservableCollection<LanguageViewModel> _languagesList = new ObservableCollection<LanguageViewModel>();
        private ObservableCollection<SportAchievementViewModel> _sportsList = new ObservableCollection<SportAchievementViewModel>();

        // Текущий выбранный формат СНИЛС
        private string _currentSnilsFormat = "space"; // "space", "hyphen", "none"

        public AdditionalInfoPage()
        {
            InitializeComponent();

            LanguagesGrid.ItemsSource = _languagesList;
            SportsGrid.ItemsSource = _sportsList;

            if (SessionManager.CurrentApplicantId.HasValue)
            {
                _currentApplicantId = SessionManager.CurrentApplicantId.Value;
            }

            // Подписка на события (часть уже была в XAML, часть добавим здесь)
            NoSnilsCheckBox.Checked += NoSnilsCheckBox_Checked;
            NoSnilsCheckBox.Unchecked += NoSnilsCheckBox_Unchecked;

            // Подписка на события формата СНИЛС (радиокнопки)
            SnilsSpaceRadio.Checked += SnilsFormatRadioButton_Checked;
            SnilsHyphenRadio.Checked += SnilsFormatRadioButton_Checked;
            SnilsNoneRadio.Checked += SnilsFormatRadioButton_Checked;

            // Подписка на события ввода СНИЛС
            SnilsTextBox.TextChanged += SnilsTextBox_TextChanged;
            SnilsTextBox.PreviewTextInput += SnilsTextBox_PreviewTextInput;

            Loaded += AdditionalInfoPage_Loaded;
        }

        private void AdditionalInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Проверяем, был ли создан абитуриент
            if (SessionManager.CurrentApplicant == null || SessionManager.CurrentApplicant.Id == 0)
            {
                MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                return;
            }

            LoadReferenceData();
            LoadApplicantAdditionalData();
            isInitialized = true;
        }

        #region Загрузка справочных данных и данных абитуриента

        private void LoadReferenceData()
        {
            try
            {
                var countries = DataService.GetByCondition<Countries>("IsActive = 1");
                BirthCountryCombo.ItemsSource = countries;
                BirthCountryCombo.DisplayMemberPath = "Name";
                BirthCountryCombo.SelectedValuePath = "Id";
                if (BirthCountryCombo.Items.Count > 0) BirthCountryCombo.SelectedIndex = 0;

                var languages = DataService.GetAll<Languages>();
                LanguageCombo.ItemsSource = languages;
                LanguageCombo.DisplayMemberPath = "Name";
                LanguageCombo.SelectedValuePath = "Id";
                if (LanguageCombo.Items.Count > 0) LanguageCombo.SelectedIndex = 0;

                var languageLevels = DataService.GetAll<LanguageLevels>().OrderBy(l => l.SortOrder).ToList();
                LanguageLevelCombo.ItemsSource = languageLevels;
                LanguageLevelCombo.DisplayMemberPath = "Name";
                LanguageLevelCombo.SelectedValuePath = "Id";
                if (LanguageLevelCombo.Items.Count > 0) LanguageLevelCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочных данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadApplicantAdditionalData()
        {
            if (_currentApplicantId == null) return;

            try
            {
                var applicant = DataService.GetApplicant(_currentApplicantId.Value);
                if (applicant == null) return;

                // Загрузка СНИЛС с форматированием
                if (!string.IsNullOrEmpty(applicant.Snils))
                {
                    string digits = new string(applicant.Snils.Where(char.IsDigit).ToArray());
                    if (digits.Length == 11)
                    {
                        SnilsTextBox.Text = FormatSnils(digits, _currentSnilsFormat);
                    }
                    else
                    {
                        SnilsTextBox.Text = applicant.Snils; // на случай, если в БД нестандарт
                    }
                    NoSnilsCheckBox.IsChecked = false;
                }
                else
                {
                    NoSnilsCheckBox.IsChecked = true;
                    SnilsTextBox.IsEnabled = false;
                }

                InnTextBox.Text = applicant.Inn ?? "";

                if (applicant.BirthCountryId.HasValue)
                    BirthCountryCombo.SelectedValue = applicant.BirthCountryId.Value;

                NeedsDormitoryCheckBox.IsChecked = applicant.NeedsDormitory ?? false;
                HasDormitoryBenefitsCheckBox.IsChecked = applicant.HasDormitoryBenefits ?? false;
                HasSportsAchievementsCheckBox.IsChecked = applicant.HasSportsAchievements ?? false;
                CompletedPreparatoryCoursesCheckBox.IsChecked = applicant.CompletedPreparatoryCourses ?? false;
                CompletedPreparatoryDepartmentCheckBox.IsChecked = applicant.CompletedPreparatoryDepartment ?? false;
                CompletedMedicalEducationCheckBox.IsChecked = applicant.CompletedMedicalEducation ?? false;
                CurrentWorkPlaceTextBox.Text = applicant.CurrentWorkPlace ?? "";

                ServedInArmyCheckBox.IsChecked = applicant.ServedInArmy ?? false;
                if (applicant.ServiceStartDate.HasValue)
                    ServiceStartDatePicker.SelectedDate = applicant.ServiceStartDate;
                if (applicant.ServiceEndDate.HasValue)
                    ServiceEndDatePicker.SelectedDate = applicant.ServiceEndDate;
                if (applicant.ReserveYear.HasValue)
                    ReserveYearTextBox.Text = applicant.ReserveYear.ToString();

                LoadLanguages(applicant.Id);
                LoadSportAchievements(applicant.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLanguages(int applicantId)
        {
            try
            {
                var languages = DataService.GetApplicantLanguages(applicantId);
                _languagesList.Clear();
                foreach (var lang in languages)
                {
                    _languagesList.Add(new LanguageViewModel
                    {
                        Id = lang.Id,
                        LanguageId = lang.LanguageId ?? 0,
                        LanguageName = GetLanguageName(lang.LanguageId),
                        LanguageLevelId = lang.LanguageLevelId ?? 0,
                        LanguageLevelName = GetLanguageLevelName(lang.LanguageLevelId),
                        IsPrimary = lang.IsPrimary ?? false
                    });
                }
                LanguagesGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки языков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetLanguageName(int? languageId)
        {
            if (languageId == null) return "";
            try
            {
                var languages = DataService.GetAll<Languages>();
                return languages.FirstOrDefault(l => l.Id == languageId)?.Name ?? "";
            }
            catch { return ""; }
        }

        private string GetLanguageLevelName(int? levelId)
        {
            if (levelId == null) return "";
            try
            {
                var levels = DataService.GetAll<LanguageLevels>();
                return levels.FirstOrDefault(l => l.Id == levelId)?.Name ?? "";
            }
            catch { return ""; }
        }

        private void LoadSportAchievements(int applicantId)
        {
            try
            {
                var sports = DataService.GetApplicantSportAchievements(applicantId);
                _sportsList.Clear();
                foreach (var sport in sports)
                {
                    _sportsList.Add(new SportAchievementViewModel
                    {
                        Id = sport.Id,
                        SportType = sport.SportType,
                        Rank = sport.Rank,
                        Year = sport.Year
                    });
                }
                SportsGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки достижений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Сохранение данных

        private bool SaveAllData()
        {
            if (isSaving) return false;

            try
            {
                isSaving = true;

                if (_currentApplicantId == null)
                {
                    MessageBox.Show("Сначала заполните данные удостоверения личности", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                var applicant = DataService.GetApplicant(_currentApplicantId.Value);
                if (applicant == null) return false;

                // Валидация ИНН
                if (!string.IsNullOrWhiteSpace(InnTextBox.Text) && !ValidationHelper.IsValidInn(InnTextBox.Text))
                {
                    MessageBox.Show("ИНН имеет неверный формат (10 или 12 цифр).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    InnTextBox.Focus();
                    return false;
                }

                // Валидация года резерва
                if (!string.IsNullOrWhiteSpace(ReserveYearTextBox.Text) && (!int.TryParse(ReserveYearTextBox.Text, out int ry) || ry < 1900 || ry > DateTime.Now.Year))
                {
                    MessageBox.Show("Год резерва указан неверно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ReserveYearTextBox.Focus();
                    return false;
                }

                // Валидация дат службы
                if (ServiceStartDatePicker.SelectedDate.HasValue && ServiceEndDatePicker.SelectedDate.HasValue)
                {
                    if (ServiceStartDatePicker.SelectedDate > ServiceEndDatePicker.SelectedDate)
                    {
                        MessageBox.Show("Дата начала службы не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                // Валидация СНИЛС (если не отмечено "Нет СНИЛС")
                if (NoSnilsCheckBox.IsChecked != true)
                {
                    string snilsDigits = new string(SnilsTextBox.Text.Where(char.IsDigit).ToArray());
                    if (snilsDigits.Length != 11)
                    {
                        MessageBox.Show("СНИЛС должен содержать ровно 11 цифр.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        SnilsTextBox.Focus();
                        return false;
                    }
                }

                FillApplicantData(applicant);
                applicant.UpdatedAt = DateTime.Now;
                DataService.UpdateApplicant(applicant);

                // Валидация языков
                foreach (var lang in _languagesList)
                {
                    if (lang.LanguageId <= 0)
                    {
                        MessageBox.Show("Выберите язык для всех записей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (lang.LanguageLevelId <= 0)
                    {
                        MessageBox.Show("Укажите уровень языка для всех записей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                SaveLanguages(applicant.Id);
                SaveSportAchievements(applicant.Id);

                SessionManager.CurrentApplicant = applicant;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                isSaving = false;
            }
        }

        private void FillApplicantData(Applicants applicant)
        {
            // СНИЛС: сохраняем только цифры (разделители игнорируются)
            if (NoSnilsCheckBox.IsChecked == true)
            {
                applicant.Snils = null;
            }
            else if (!string.IsNullOrWhiteSpace(SnilsTextBox.Text))
            {
                applicant.Snils = new string(SnilsTextBox.Text.Where(char.IsDigit).ToArray());
            }

            applicant.Inn = !string.IsNullOrWhiteSpace(InnTextBox.Text) ?
                new string(InnTextBox.Text.Where(char.IsDigit).ToArray()) : null;

            if (BirthCountryCombo.SelectedValue != null)
                applicant.BirthCountryId = (int)BirthCountryCombo.SelectedValue;

            applicant.NeedsDormitory = NeedsDormitoryCheckBox.IsChecked;
            applicant.HasDormitoryBenefits = HasDormitoryBenefitsCheckBox.IsChecked;
            applicant.HasSportsAchievements = HasSportsAchievementsCheckBox.IsChecked;
            applicant.CompletedPreparatoryCourses = CompletedPreparatoryCoursesCheckBox.IsChecked;
            applicant.CompletedPreparatoryDepartment = CompletedPreparatoryDepartmentCheckBox.IsChecked;
            applicant.CompletedMedicalEducation = CompletedMedicalEducationCheckBox.IsChecked;
            applicant.CurrentWorkPlace = !string.IsNullOrWhiteSpace(CurrentWorkPlaceTextBox.Text) ?
                CurrentWorkPlaceTextBox.Text.Trim() : null;

            applicant.ServedInArmy = ServedInArmyCheckBox.IsChecked;
            applicant.ServiceStartDate = ServiceStartDatePicker.SelectedDate;
            applicant.ServiceEndDate = ServiceEndDatePicker.SelectedDate;

            if (!string.IsNullOrWhiteSpace(ReserveYearTextBox.Text) && int.TryParse(ReserveYearTextBox.Text, out int year))
            {
                applicant.ReserveYear = year;
            }
            else
            {
                applicant.ReserveYear = null;
            }
        }

        private void SaveLanguages(int applicantId)
        {
            try
            {
                var existingLanguages = DataService.GetApplicantLanguages(applicantId);
                var keptIds = _languagesList.Where(l => l.Id > 0).Select(l => l.Id).ToList();

                // Удаляем удаленные
                foreach (var lang in existingLanguages.Where(el => !keptIds.Contains(el.Id)))
                {
                    DataService.DeleteApplicantLanguage(lang.Id);
                }

                // Обновляем и добавляем
                foreach (var langVM in _languagesList)
                {
                    if (langVM.Id > 0)
                    {
                        var existing = existingLanguages.FirstOrDefault(el => el.Id == langVM.Id);
                        if (existing != null)
                        {
                            existing.LanguageId = langVM.LanguageId;
                            existing.LanguageLevelId = langVM.LanguageLevelId;
                            existing.IsPrimary = langVM.IsPrimary;
                            DataService.UpdateApplicantLanguage(existing);
                        }
                    }
                    else
                    {
                        DataService.CreateApplicantLanguage(
                            applicantId,
                            langVM.LanguageId,
                            langVM.LanguageLevelId,
                            langVM.IsPrimary);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения языков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSportAchievements(int applicantId)
        {
            try
            {
                var existingSports = DataService.GetApplicantSportAchievements(applicantId);
                var keptIds = _sportsList.Where(s => s.Id > 0).Select(s => s.Id).ToList();

                foreach (var sport in existingSports.Where(es => !keptIds.Contains(es.Id)))
                {
                    DataService.DeleteSportAchievement(sport.Id);
                }

                foreach (var sportVM in _sportsList)
                {
                    if (sportVM.Id > 0)
                    {
                        var existing = existingSports.FirstOrDefault(es => es.Id == sportVM.Id);
                        if (existing != null)
                        {
                            existing.SportType = sportVM.SportType;
                            existing.Rank = sportVM.Rank;
                            existing.Year = sportVM.Year;
                            DataService.UpdateSportAchievement(existing);
                        }
                    }
                    else
                    {
                        DataService.CreateSportAchievement(
                            applicantId,
                            sportVM.SportType,
                            null, // achievement
                            sportVM.Rank,
                            sportVM.Year);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения достижений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Обработчики событий для СНИЛС (маска ввода)

        // Форматирование СНИЛС согласно выбранному формату
        private string FormatSnils(string digits, string format)
        {
            if (digits.Length != 11) return digits; // без разделителей
            switch (format)
            {
                case "space":
                    return $"{digits.Substring(0, 3)} {digits.Substring(3, 3)} {digits.Substring(6, 3)} {digits.Substring(9, 2)}";
                case "hyphen":
                    return $"{digits.Substring(0, 3)}-{digits.Substring(3, 3)}-{digits.Substring(6, 3)} {digits.Substring(9, 2)}";
                case "none":
                default:
                    return digits;
            }
        }

        // Обработчик изменения текста СНИЛС — применяет маску
        private void SnilsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NoSnilsCheckBox.IsChecked == true) return;
            if (_isUpdatingSnils) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Сохраняем позицию курсора
            int caretIndex = textBox.CaretIndex;

            // Удаляем все нецифровые символы
            string digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Ограничиваем 11 цифрами
            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            // Форматируем согласно текущему формату
            string formatted = FormatSnils(digits, _currentSnilsFormat);

            if (textBox.Text != formatted)
            {
                _isUpdatingSnils = true;
                textBox.Text = formatted;
                // Восстанавливаем позицию курсора
                int newCaret = Math.Min(caretIndex, formatted.Length);
                // Если курсор попал на разделитель, лучше сдвинуть на цифру вперёд
                // Для простоты оставляем как есть
                textBox.CaretIndex = newCaret;
                _isUpdatingSnils = false;
            }
        }

        // Разрешаем ввод только цифр
        private void SnilsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        // Обработчик смены формата (радиокнопки) — переформатировать, если 11 цифр
        private void SnilsFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SnilsSpaceRadio.IsChecked == true)
                _currentSnilsFormat = "space";
            else if (SnilsHyphenRadio.IsChecked == true)
                _currentSnilsFormat = "hyphen";
            else if (SnilsNoneRadio.IsChecked == true)
                _currentSnilsFormat = "none";

            // Если в поле есть 11 цифр, переформатировать
            string digits = new string(SnilsTextBox.Text.Where(char.IsDigit).ToArray());
            if (digits.Length == 11)
            {
                SnilsTextBox.Text = FormatSnils(digits, _currentSnilsFormat);
                SnilsTextBox.CaretIndex = SnilsTextBox.Text.Length;
            }
        }

        // Обработчики для чекбокса "Нет СНИЛС"
        private void NoSnilsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SnilsTextBox.IsEnabled = false;
            SnilsTextBox.Text = string.Empty;
        }

        private void NoSnilsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SnilsTextBox.IsEnabled = true;
            SnilsTextBox.Focus();
        }

        #endregion

        #region Остальные валидационные обработчики

        private void InnTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InnTextBox.Text))
            {
                if (!ValidationHelper.IsValidInn(InnTextBox.Text))
                {
                    MessageBox.Show("ИНН имеет неверный формат. Требуется 10 или 12 цифр", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    InnTextBox.Focus();
                }
            }
        }

        private void ReserveYearTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ReserveYearTextBox.Text))
            {
                if (!int.TryParse(ReserveYearTextBox.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                {
                    MessageBox.Show($"Год должен быть от 1900 до {DateTime.Now.Year}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void InnTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void ReserveYearTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        #endregion

        #region Обработчики для языков и спорта

        private void AddLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageCombo.SelectedItem == null || LanguageLevelCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите язык и уровень владения", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedLanguage = LanguageCombo.SelectedItem as Languages;
            var selectedLevel = LanguageLevelCombo.SelectedItem as LanguageLevels;

            if (selectedLanguage == null || selectedLevel == null) return;

            if (_languagesList.Any(l => l.LanguageId == selectedLanguage.Id))
            {
                MessageBox.Show("Этот язык уже добавлен", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsPrimaryLanguageCheckBox.IsChecked == true)
            {
                foreach (var lang in _languagesList)
                    lang.IsPrimary = false;
            }

            _languagesList.Add(new LanguageViewModel
            {
                Id = 0,
                LanguageId = selectedLanguage.Id,
                LanguageName = selectedLanguage.Name,
                LanguageLevelId = selectedLevel.Id,
                LanguageLevelName = selectedLevel.Name,
                IsPrimary = IsPrimaryLanguageCheckBox.IsChecked ?? false
            });

            LanguagesGrid.Items.Refresh();

            LanguageCombo.SelectedIndex = 0;
            LanguageLevelCombo.SelectedIndex = 0;
            IsPrimaryLanguageCheckBox.IsChecked = false;
        }

        private void DeleteLanguage_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is LanguageViewModel language)
            {
                _languagesList.Remove(language);
                LanguagesGrid.Items.Refresh();
            }
        }

        private void AddSportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SportTypeTextBox.Text))
            {
                MessageBox.Show("Введите вид спорта", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? year = null;
            if (!string.IsNullOrWhiteSpace(SportYearTextBox.Text))
            {
                if (int.TryParse(SportYearTextBox.Text, out int parsedYear) &&
                    parsedYear >= 1900 && parsedYear <= DateTime.Now.Year + 1)
                {
                    year = parsedYear;
                }
                else
                {
                    MessageBox.Show($"Введите корректный год (1900-{DateTime.Now.Year + 1})",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _sportsList.Add(new SportAchievementViewModel
            {
                Id = 0,
                SportType = SportTypeTextBox.Text.Trim(),
                Rank = SportRankTextBox.Text?.Trim(),
                Year = year
            });

            SportsGrid.Items.Refresh();

            SportTypeTextBox.Clear();
            SportRankTextBox.Clear();
            SportYearTextBox.Clear();
        }

        private void DeleteSport_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SportAchievementViewModel sport)
            {
                _sportsList.Remove(sport);
                SportsGrid.Items.Refresh();
            }
        }

        #endregion

        #region Навигационные кнопки

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveAllData())
            {
                NavigationService?.Navigate(new ApplicationCompetitionsPage());
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите отменить изменения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SessionManager.Clear();
                if (NavigationService?.CanGoBack == true)
                {
                    while (NavigationService.CanGoBack) NavigationService.GoBack();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }

        #endregion
    }

    // Вспомогательные классы (если не определены в другом месте)
    public class LanguageViewModel
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public int LanguageLevelId { get; set; }
        public string LanguageLevelName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class SportAchievementViewModel
    {
        public int Id { get; set; }
        public string SportType { get; set; }
        public string Rank { get; set; }
        public int? Year { get; set; }
    }
}