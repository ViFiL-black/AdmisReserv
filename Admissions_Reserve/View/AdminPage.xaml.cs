using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;
using Microsoft.Win32;
using System.IO;

namespace Admissions_Reserve.View
{
    public partial class AdminPage : Page
    {
        // Пользователи
        public ObservableCollection<UserDisplay> Users { get; set; }

        // Конкурсы
        private ObservableCollection<Competitions> _competitions;
        private Competitions _selectedCompetition;
        private bool _isEditingCompetition = false;

        // Справочные данные для ComboBox
        private ObservableCollection<BaseEducationLevels> _baseEducationLevels;
        private ObservableCollection<StudyForms> _studyForms;
        private ObservableCollection<AdmissionTypes> _admissionTypes;
        private ObservableCollection<Departments> _departments;
        private ObservableCollection<Branches> _branches;

        public AdminPage()
        {
            InitializeComponent();
            Users = new ObservableCollection<UserDisplay>();
            UsersDataGrid.ItemsSource = Users;
            _competitions = new ObservableCollection<Competitions>();
            CompetitionsDataGrid.ItemsSource = _competitions;

            LoadRoles();
            LoadUsers();
            LoadReferenceData();
            LoadCompetitions();
        }

        // ===================== ПОЛЬЗОВАТЕЛИ =====================

        private void LoadRoles()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = "SELECT Id, Name FROM Roles ORDER BY Name";
                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RoleCombo.Items.Add(new ComboBoxItem
                            {
                                Content = reader["Name"].ToString(),
                                Tag = Convert.ToInt32(reader["Id"])
                            });
                        }
                    }
                }
                if (RoleCombo.Items.Count > 0)
                    RoleCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка загрузки ролей: {ex.Message}", false);
            }
        }

        private void LoadUsers()
        {
            Users.Clear();
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"
                        SELECT u.Id, u.Login, u.FullName, u.CreatedAt, r.Name AS RoleName
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.Id
                        ORDER BY u.Id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Users.Add(new UserDisplay
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Login = reader["Login"]?.ToString(),
                                FullName = reader["FullName"]?.ToString(),
                                RoleName = reader["RoleName"]?.ToString() ?? "Нет роли",
                                CreatedAt = reader["CreatedAt"] != DBNull.Value
                                    ? DateTime.Parse(reader["CreatedAt"].ToString())
                                    : DateTime.MinValue
                            });
                        }
                    }
                }
                SetStatus($"Загружено пользователей: {Users.Count}", true);
            }
            catch (Exception ex)
            {
                SetStatus($"Ошибка загрузки пользователей: {ex.Message}", false);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string fullName = FullNameTextBox.Text.Trim();
            string selectedRole = (RoleCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(login))
            {
                SetStatus("Ошибка: Логин обязателен.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                SetStatus("Ошибка: Пароль обязателен.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(fullName))
            {
                SetStatus("Ошибка: ФИО обязательно.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                SetStatus("Ошибка: Выберите роль.", false);
                return;
            }

            if (IsLoginExists(login))
            {
                SetStatus("Ошибка: Пользователь с таким логином уже существует.", false);
                return;
            }

            bool success = UserAuthentication.CreateUser(login, password, fullName, selectedRole);
            if (success)
            {
                SetStatus($"Пользователь '{login}' успешно добавлен с ролью '{selectedRole}'.", true);
                ClearForm();
                LoadUsers();
            }
            else
            {
                SetStatus("Ошибка при создании пользователя. Проверьте логи и параметры.", false);
            }
        }

        private bool IsLoginExists(string login)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = "SELECT COUNT(*) FROM Users WHERE Login = @Login";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return true;
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is UserDisplay userToDelete)
            {
                if (UserAuthentication.CurrentUserId == userToDelete.Id)
                {
                    SetStatus("Нельзя удалить текущего пользователя.", false);
                    return;
                }

                if (MessageBox.Show($"Удалить пользователя '{userToDelete.Login}'?",
                                    "Подтверждение удаления",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = DatabaseHelper.GetConnection())
                        {
                            var query = "DELETE FROM Users WHERE Id = @Id";
                            using (var cmd = new SQLiteCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@Id", userToDelete.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        SetStatus($"Пользователь '{userToDelete.Login}' удалён.", true);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        SetStatus($"Ошибка удаления: {ex.Message}", false);
                    }
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetStatus("Поля очищены.", true);
        }

        private void ClearForm()
        {
            LoginTextBox.Text = "";
            PasswordBox.Password = "";
            FullNameTextBox.Text = "";
            if (RoleCombo.Items.Count > 0)
                RoleCombo.SelectedIndex = 0;
        }

        private void SetStatus(string message, bool isSuccess)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isSuccess
                ? System.Windows.Media.Brushes.Green
                : System.Windows.Media.Brushes.Red;
        }

        // ===================== СПРАВОЧНЫЕ ДАННЫЕ ДЛЯ КОНКУРСОВ =====================

        private void LoadReferenceData()
        {
            try
            {
                _baseEducationLevels = new ObservableCollection<BaseEducationLevels>(DataService.GetAll<BaseEducationLevels>());
                _studyForms = new ObservableCollection<StudyForms>(DataService.GetAll<StudyForms>());
                _admissionTypes = new ObservableCollection<AdmissionTypes>(DataService.GetAll<AdmissionTypes>());
                _departments = new ObservableCollection<Departments>(DataService.GetAll<Departments>());
                _branches = new ObservableCollection<Branches>(DataService.GetAll<Branches>());

                CompetitionBaseCombo.ItemsSource = _baseEducationLevels;
                CompetitionFormCombo.ItemsSource = _studyForms;
                CompetitionAdmissionTypeCombo.ItemsSource = _admissionTypes;
                CompetitionDepartmentCombo.ItemsSource = _departments;
                CompetitionBranchCombo.ItemsSource = _branches;

                // Если в списках есть элементы, выбираем первый (по умолчанию)
                if (_baseEducationLevels.Count > 0) CompetitionBaseCombo.SelectedIndex = 0;
                if (_studyForms.Count > 0) CompetitionFormCombo.SelectedIndex = 0;
                if (_admissionTypes.Count > 0) CompetitionAdmissionTypeCombo.SelectedIndex = 0;
                if (_departments.Count > 0) CompetitionDepartmentCombo.SelectedIndex = 0;
                if (_branches.Count > 0) CompetitionBranchCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                CompetitionStatusTextBlock.Text = $"Ошибка загрузки справочников: {ex.Message}";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // ===================== КОНКУРСЫ =====================

        private void LoadCompetitions()
        {
            _competitions.Clear();
            try
            {
                var list = DataService.GetByCondition<Competitions>("1=1");
                foreach (var comp in list)
                    _competitions.Add(comp);
                CompetitionStatusTextBlock.Text = $"Загружено конкурсов: {_competitions.Count}";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                CompetitionStatusTextBlock.Text = $"Ошибка загрузки конкурсов: {ex.Message}";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ClearCompetitionForm()
        {
            CompetitionNameTextBox.Text = "";
            CompetitionBaseCombo.SelectedIndex = -1;
            CompetitionFormCombo.SelectedIndex = -1;
            CompetitionAdmissionTypeCombo.SelectedIndex = -1;
            CompetitionDepartmentCombo.SelectedIndex = -1;
            CompetitionBranchCombo.SelectedIndex = -1;
            CompetitionIsActiveCheckBox.IsChecked = true;
            _selectedCompetition = null;
            _isEditingCompetition = false;
            AddCompetitionButton.IsEnabled = true;
            UpdateCompetitionButton.IsEnabled = false;
        }

        private void FillCompetitionForm(Competitions comp)
        {
            CompetitionNameTextBox.Text = comp.Name;
            SetComboBoxValue(CompetitionBaseCombo, comp.EducationBase);
            SetComboBoxValue(CompetitionFormCombo, comp.StudyForm);
            SetComboBoxValue(CompetitionAdmissionTypeCombo, comp.AdmissionType);
            SetComboBoxValue(CompetitionDepartmentCombo, comp.Department);
            SetComboBoxValue(CompetitionBranchCombo, comp.Branch);
            CompetitionIsActiveCheckBox.IsChecked = comp.IsActive;
            _selectedCompetition = comp;
            _isEditingCompetition = true;
            AddCompetitionButton.IsEnabled = false;
            UpdateCompetitionButton.IsEnabled = true;
        }

        private void SetComboBoxValue(ComboBox combo, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                combo.SelectedIndex = -1;
                return;
            }
            foreach (var item in combo.Items)
            {
                var prop = item.GetType().GetProperty(combo.SelectedValuePath);
                if (prop != null)
                {
                    var val = prop.GetValue(item)?.ToString();
                    if (val == value)
                    {
                        combo.SelectedItem = item;
                        return;
                    }
                }
            }
            combo.SelectedIndex = -1;
        }

        private void AddCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompetitionNameTextBox.Text))
            {
                CompetitionStatusTextBlock.Text = "Ошибка: Название конкурса обязательно.";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                var comp = new Competitions
                {
                    Name = CompetitionNameTextBox.Text.Trim(),
                    EducationBase = (CompetitionBaseCombo.SelectedItem as BaseEducationLevels)?.Name ?? "",
                    StudyForm = (CompetitionFormCombo.SelectedItem as StudyForms)?.Name ?? "",
                    AdmissionType = (CompetitionAdmissionTypeCombo.SelectedItem as AdmissionTypes)?.Name ?? "",
                    Department = (CompetitionDepartmentCombo.SelectedItem as Departments)?.Name ?? "",
                    Branch = (CompetitionBranchCombo.SelectedItem as Branches)?.Name ?? "",
                    IsActive = CompetitionIsActiveCheckBox.IsChecked == true
                };
                int id = DataService.CreateCompetition(comp);
                comp.Id = id;
                _competitions.Add(comp);
                CompetitionStatusTextBlock.Text = $"Конкурс '{comp.Name}' добавлен.";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                ClearCompetitionForm();
            }
            catch (Exception ex)
            {
                CompetitionStatusTextBlock.Text = $"Ошибка добавления: {ex.Message}";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void UpdateCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCompetition == null) return;
            if (string.IsNullOrWhiteSpace(CompetitionNameTextBox.Text))
            {
                CompetitionStatusTextBlock.Text = "Ошибка: Название конкурса обязательно.";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                _selectedCompetition.Name = CompetitionNameTextBox.Text.Trim();
                _selectedCompetition.EducationBase = (CompetitionBaseCombo.SelectedItem as BaseEducationLevels)?.Name ?? "";
                _selectedCompetition.StudyForm = (CompetitionFormCombo.SelectedItem as StudyForms)?.Name ?? "";
                _selectedCompetition.AdmissionType = (CompetitionAdmissionTypeCombo.SelectedItem as AdmissionTypes)?.Name ?? "";
                _selectedCompetition.Department = (CompetitionDepartmentCombo.SelectedItem as Departments)?.Name ?? "";
                _selectedCompetition.Branch = (CompetitionBranchCombo.SelectedItem as Branches)?.Name ?? "";
                _selectedCompetition.IsActive = CompetitionIsActiveCheckBox.IsChecked == true;

                DataService.UpdateCompetition(_selectedCompetition);
                CompetitionStatusTextBlock.Text = $"Конкурс '{_selectedCompetition.Name}' обновлён.";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                ClearCompetitionForm();
                LoadCompetitions(); // перезагружаем список
            }
            catch (Exception ex)
            {
                CompetitionStatusTextBlock.Text = $"Ошибка обновления: {ex.Message}";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void CancelCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCompetitionForm();
            CompetitionStatusTextBlock.Text = "Редактирование отменено.";
            CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void CompetitionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCompetition = CompetitionsDataGrid.SelectedItem as Competitions;
        }

        private void EditCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var comp = btn?.Tag as Competitions;
            if (comp != null)
            {
                FillCompetitionForm(comp);
                CompetitionStatusTextBlock.Text = $"Редактирование конкурса '{comp.Name}'";
                CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;
            }
        }

        private void DeleteCompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var comp = btn?.Tag as Competitions;
            if (comp != null)
            {
                if (MessageBox.Show($"Удалить конкурс '{comp.Name}'?",
                                    "Подтверждение удаления",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        DataService.DeleteCompetition(comp.Id);
                        _competitions.Remove(comp);
                        CompetitionStatusTextBlock.Text = $"Конкурс '{comp.Name}' удалён.";
                        CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                        if (_selectedCompetition == comp)
                            ClearCompetitionForm();
                    }
                    catch (Exception ex)
                    {
                        CompetitionStatusTextBlock.Text = $"Ошибка удаления: {ex.Message}";
                        CompetitionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
        }

        // ===================== ОБЩИЕ МЕТОДЫ =====================

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                MessageBox.Show("Нет предыдущей страницы для возврата.", "Навигация");
        }

        private void CreateDbButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Создать новую базу данных",
                Filter = "SQLite database (*.db)|*.db",
                DefaultExt = ".db",
                FileName = $"AdmissionsReserve_{DateTime.Now:yyyyMMdd_HHmmss}.db"
            };
            if (dialog.ShowDialog() == true)
            {
                string newDbPath = dialog.FileName;
                try
                {
                    DatabaseHelper.CreateNewDatabase(newDbPath);
                    SetStatus($"База данных создана: {newDbPath}", true);
                    if (MessageBox.Show($"Подключиться к новой базе данных?\n{newDbPath}",
                        "Переключение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        DatabaseHelper.SwitchDatabase(newDbPath);
                        LoadUsers();
                        LoadCompetitions();
                        SetStatus($"Подключено к: {newDbPath}", true);
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Ошибка создания БД: {ex.Message}", false);
                }
            }
        }

        private void ConnectDbButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл базы данных SQLite",
                Filter = "SQLite files (*.db)|*.db|All files (*.*)|*.*",
                DefaultExt = ".db"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DatabaseHelper.SwitchDatabase(dialog.FileName);
                    LoadUsers();
                    LoadCompetitions();
                    SetStatus($"Подключено к: {dialog.FileName}", true);
                }
                catch (Exception ex)
                {
                    SetStatus($"Ошибка подключения: {ex.Message}", false);
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Закрыть приложение?", "Выход",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }

    // Модель для отображения пользователей
    public class UserDisplay : INotifyPropertyChanged
    {
        private int _id;
        private string _login;
        private string _fullName;
        private string _roleName;
        private DateTime _createdAt;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string Login { get => _login; set { _login = value; OnPropertyChanged(); } }
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }
        public string RoleName { get => _roleName; set { _roleName = value; OnPropertyChanged(); } }
        public DateTime CreatedAt { get => _createdAt; set { _createdAt = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}