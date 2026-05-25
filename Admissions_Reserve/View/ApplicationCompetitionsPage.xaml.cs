using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class ApplicationCompetitionsPage : Page
    {
        // Модель конкурса
        public class CompetitionItem : INotifyPropertyChanged
        {
            private bool _isSelected;
            private int _priority;
            private string _programName;
            private string _educationBase;
            private string _studyForm;
            private string _admissionType;
            private string _otherCharacter;
            private string _branch;
            private string _department;
            private string _additionalConditions;

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
            }
            public int Priority
            {
                get => _priority;
                set { _priority = value; OnPropertyChanged(nameof(Priority)); }
            }
            public string ProgramName
            {
                get => _programName;
                set { _programName = value; OnPropertyChanged(nameof(ProgramName)); }
            }
            public string EducationBase
            {
                get => _educationBase;
                set { _educationBase = value; OnPropertyChanged(nameof(EducationBase)); }
            }
            public string StudyForm
            {
                get => _studyForm;
                set { _studyForm = value; OnPropertyChanged(nameof(StudyForm)); }
            }
            public string AdmissionType
            {
                get => _admissionType;
                set { _admissionType = value; OnPropertyChanged(nameof(AdmissionType)); }
            }
            public string OtherCharacter
            {
                get => _otherCharacter;
                set { _otherCharacter = value; OnPropertyChanged(nameof(OtherCharacter)); }
            }
            public string Branch
            {
                get => _branch;
                set { _branch = value; OnPropertyChanged(nameof(Branch)); }
            }
            public string Department
            {
                get => _department;
                set { _department = value; OnPropertyChanged(nameof(Department)); }
            }
            public string AdditionalConditions
            {
                get => _additionalConditions;
                set { _additionalConditions = value; OnPropertyChanged(nameof(AdditionalConditions)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<CompetitionItem> _availableCompetitions;
        private ObservableCollection<CompetitionItem> _selectedCompetitions;
        private int _nextPriority = 1;
        private bool isInitialized = false;

        public ApplicationCompetitionsPage()
        {
            InitializeComponent();
            InitializeData();
            isInitialized = true;
        }

        private void InitializeData()
        {
            _availableCompetitions = new ObservableCollection<CompetitionItem>();
            _selectedCompetitions = new ObservableCollection<CompetitionItem>();

            LoadAvailableCompetitions();

            AvailableCompetitionsGrid.ItemsSource = _availableCompetitions;
            SelectedCompetitionsGrid.ItemsSource = _selectedCompetitions;
        }

        private void LoadAvailableCompetitions()
        {
            var programs = new[]
            {
                new CompetitionItem
                {
                    ProgramName = "08.02.09 Монтаж, наладка и эксплуатация электрооборудования промышленных и гражданских зданий",
                    EducationBase = "Осн. общ.", StudyForm = "Очная", AdmissionType = "Общий",
                    OtherCharacter = "Головная орг.", Branch = "Головная орг.", Department = "Отделение автоматики",
                    IsSelected = false
                },
                new CompetitionItem
                {
                    ProgramName = "15.02.17 Монтаж, техническое обслуживание, эксплуатация и ремонт промышленного оборудования (по отраслям)",
                    EducationBase = "Осн. общ.", StudyForm = "Очная", AdmissionType = "Общий",
                    OtherCharacter = "Головная орг.", Branch = "Головная орг.", Department = "Отделение автоматики и электромеханики",
                    IsSelected = false
                },
                new CompetitionItem
                {
                    ProgramName = "27.02.04 Автоматические системы управления",
                    EducationBase = "Осн. общ.", StudyForm = "Очная", AdmissionType = "Общий",
                    OtherCharacter = "Головная орг.", Branch = "Головная орг.",
                    Department = "Отделение автоматики и электромеханики (в том числе с применением автоматических систем управления)",
                    IsSelected = false
                },
                new CompetitionItem
                {
                    ProgramName = "09.02.11 Разработка и управление программным обеспечением",
                    EducationBase = "Осн. общ.", StudyForm = "Очная", AdmissionType = "Общий",
                    OtherCharacter = "Головная орг.", Branch = "Головная орг.", Department = "Отделение автоматики и электромеханики",
                    IsSelected = false
                }
            };

            foreach (var program in programs)
                _availableCompetitions.Add(program);
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            // Здесь можно добавить логику фильтрации
            MessageBox.Show("Фильтры применены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AvailableCompetitionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isInitialized) return;
            if (e.EditAction != DataGridEditAction.Commit) return;

            var item = e.Row.Item as CompetitionItem;
            if (item == null) return;

            if (item.IsSelected)
            {
                // Проверяем, не добавлен ли уже
                bool exists = _selectedCompetitions.Any(s => s.ProgramName == item.ProgramName);

                if (!exists)
                {
                    _selectedCompetitions.Add(new CompetitionItem
                    {
                        Priority = _nextPriority++,
                        ProgramName = item.ProgramName,
                        EducationBase = item.EducationBase,
                        StudyForm = item.StudyForm,
                        AdmissionType = item.AdmissionType,
                        OtherCharacter = item.OtherCharacter,
                        Branch = item.Branch,
                        Department = item.Department,
                        AdditionalConditions = "текущее",
                        IsSelected = true
                    });
                }
            }
            else
            {
                // Удаляем из выбранных
                var toRemove = _selectedCompetitions.FirstOrDefault(s => s.ProgramName == item.ProgramName);
                if (toRemove != null)
                {
                    _selectedCompetitions.Remove(toRemove);
                    RenumberPriorities();
                }
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            var button = sender as Button;
            var item = button?.Tag as CompetitionItem;

            if (item != null)
            {
                int currentIndex = _selectedCompetitions.IndexOf(item);
                if (currentIndex > 0)
                {
                    _selectedCompetitions.Move(currentIndex, currentIndex - 1);
                    RenumberPriorities();
                }
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            var button = sender as Button;
            var item = button?.Tag as CompetitionItem;

            if (item != null)
            {
                int currentIndex = _selectedCompetitions.IndexOf(item);
                if (currentIndex < _selectedCompetitions.Count - 1)
                {
                    _selectedCompetitions.Move(currentIndex, currentIndex + 1);
                    RenumberPriorities();
                }
            }
        }

        private void DeleteSelectedCompetition_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            var button = sender as Button;
            var item = button?.Tag as CompetitionItem;

            if (item != null)
            {
                var result = MessageBox.Show($"Удалить конкурс \"{item.ProgramName}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _selectedCompetitions.Remove(item);
                    RenumberPriorities();

                    // Снимаем отметку в таблице доступных
                    var available = _availableCompetitions.FirstOrDefault(a => a.ProgramName == item.ProgramName);
                    if (available != null)
                        available.IsSelected = false;
                }
            }
        }

        private void RenumberPriorities()
        {
            int priority = 1;
            foreach (var item in _selectedCompetitions)
            {
                item.Priority = priority++;
            }
            _nextPriority = priority;
            SelectedCompetitionsGrid.Items.Refresh();
        }

        private void HasPrivilegeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            PrivilegeCategoryCombo.IsEnabled = true;
            PrivilegeDocumentsTextBox.IsEnabled = true;
        }

        private void HasPrivilegeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            PrivilegeCategoryCombo.IsEnabled = false;
            PrivilegeDocumentsTextBox.IsEnabled = false;
            PrivilegeCategoryCombo.SelectedIndex = 0;
            PrivilegeDocumentsTextBox.Text = "";
        }

        // Получение данных
        public ApplicationData GetApplicationData()
        {
            return new ApplicationData
            {
                Stage = (StageCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                CostReimbursement = (CostReimbursementCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                StudyForm = (StudyFormCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Department = (DepartmentCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Direction = (DirectionCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                BaseLevel = (BaseLevelCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                AdmissionType = (AdmissionTypeCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                TargetAdmission = (TargetAdmissionCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                HasPrivilege = HasPrivilegeCheckBox.IsChecked == true,
                PrivilegeCategory = (PrivilegeCategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                PrivilegeDocuments = PrivilegeDocumentsTextBox.Text,
                SelectedCompetitions = new ObservableCollection<CompetitionItem>(_selectedCompetitions),
                Comment = CommentTextBox.Text
            };
        }

        // Валидация
        public bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(DirectionCombo.Text))
            {
                MessageBox.Show("Пожалуйста, укажите направление подготовки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_selectedCompetitions.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один конкурс", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (HasPrivilegeCheckBox.IsChecked == true && string.IsNullOrWhiteSpace(PrivilegeDocumentsTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите документы, подтверждающие преимущественное право",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Сохранение данных конкурсов в БД
        private bool SaveData()
        {
            try
            {
                if (SessionManager.CurrentApplicantId == null)
                {
                    MessageBox.Show("Ошибка: данные абитуриента не найдены", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Сохраняем выбранные конкурсы в базу данных
                int priorityOrder = 1;
                foreach (var competition in _selectedCompetitions)
                {
                    var existingCompetition = DataService.GetApplicantCompetitions(SessionManager.CurrentApplicantId.Value)
                        .FirstOrDefault(c => c.CompetitionName == competition.ProgramName);

                    if (existingCompetition == null)
                    {
                        // Создаем новую запись
                        DataService.CreateCompetitionPriority(
                            SessionManager.CurrentApplicantId.Value,
                            competition.ProgramName ?? "",
                            priorityOrder
                        );
                    }
                    else
                    {
                        // Обновляем существующую запись
                        existingCompetition.PriorityOrder = priorityOrder;
                        DataService.UpdateCompetitionPriority(existingCompetition);
                    }

                    priorityOrder++;
                }

                DataService.LogChange("CompetitionPriorities", SessionManager.CurrentApplicantId.Value, "UPDATE");

                MessageBox.Show("Данные о конкурсах успешно сохранены!", "Успех",
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

        // Кнопка ДАЛЕЕ - переход на страницу приоритетов
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }

            try
            {
                if (SaveData())
                {
                    await System.Threading.Tasks.Task.Delay(100);
                    NavigationService?.Navigate(new PrioritiesPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переходе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (button != null)
                {
                    button.IsEnabled = true;
                }
            }
        }

        // Кнопка НАЗАД - возврат на страницу документов
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();

            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        // Кнопка ОТМЕНИТЬ
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
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

    // Класс для хранения данных заявления
    public class ApplicationData
    {
        public string Stage { get; set; }
        public string CostReimbursement { get; set; }
        public string StudyForm { get; set; }
        public string Department { get; set; }
        public string Direction { get; set; }
        public string BaseLevel { get; set; }
        public string AdmissionType { get; set; }
        public string TargetAdmission { get; set; }
        public bool HasPrivilege { get; set; }
        public string PrivilegeCategory { get; set; }
        public string PrivilegeDocuments { get; set; }
        public ObservableCollection<ApplicationCompetitionsPage.CompetitionItem> SelectedCompetitions { get; set; }
        public string Comment { get; set; }
    }
}