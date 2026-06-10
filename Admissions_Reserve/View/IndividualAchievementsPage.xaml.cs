using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class IndividualAchievementsPage : Page
    {
        public class IndividualAchievement : INotifyPropertyChanged
        {
            private int _id;
            private int _number;
            private string _category;
            private string _achievementName;
            private string _year;
            private int _points;
            private string _documentName;
            private string _documentPath;

            public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
            public int Number { get => _number; set { _number = value; OnPropertyChanged(nameof(Number)); } }
            public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }
            public string AchievementName { get => _achievementName; set { _achievementName = value; OnPropertyChanged(nameof(AchievementName)); } }
            public string Year { get => _year; set { _year = value; OnPropertyChanged(nameof(Year)); } }
            public int Points { get => _points; set { _points = value; OnPropertyChanged(nameof(Points)); } }
            public string DocumentName { get => _documentName; set { _documentName = value; OnPropertyChanged(nameof(DocumentName)); } }
            public string DocumentPath { get => _documentPath; set { _documentPath = value; OnPropertyChanged(nameof(DocumentPath)); OnPropertyChanged(nameof(HasFile)); } }

            public Visibility HasFile => !string.IsNullOrEmpty(DocumentPath) && File.Exists(DocumentPath) ? Visibility.Visible : Visibility.Collapsed;

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<IndividualAchievement> _achievements;
        private string _uploadedFilePath;
        private int _nextNumber = 1;
        private bool isSaving = false;
        private readonly string _documentsRootPath;

        public IndividualAchievementsPage()
        {
            InitializeComponent();
            _documentsRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "AchievementDocuments");
            if (!Directory.Exists(_documentsRootPath))
                Directory.CreateDirectory(_documentsRootPath);

            _achievements = new ObservableCollection<IndividualAchievement>();
            AchievementsGrid.ItemsSource = _achievements;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentApplicant == null || SessionManager.CurrentApplicant.Id == 0)
            {
                MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                return;
            }

            LoadAchievementTypes();
            LoadAchievementsFromDatabase();
        }

        private void LoadAchievementTypes()
        {
            try
            {
                var types = DataService.GetAll<IndividualAchievementTypes>();
                CategoryCombo.Items.Clear();
                foreach (var type in types)
                {
                    CategoryCombo.Items.Add(new ComboBoxItem
                    {
                        Content = type.Name,
                        Tag = type.DefaultPoints
                    });
                }
                if (CategoryCombo.Items.Count > 0) CategoryCombo.SelectedIndex = 0;
            }
            catch
            {
                var defaults = new[] { "Победитель всероссийской олимпиады школьников", "Призер всероссийской олимпиады школьников", "Значок ГТО (золотой)", "Аттестат с отличием" };
                foreach (var d in defaults)
                    CategoryCombo.Items.Add(new ComboBoxItem { Content = d });
                if (CategoryCombo.Items.Count > 0) CategoryCombo.SelectedIndex = 0;
            }
        }

        private void LoadAchievementsFromDatabase()
        {
            try
            {
                if (SessionManager.CurrentApplicantId == null) return;
                _achievements.Clear();

                var achievements = DatabasePersistenceHelper.LoadIndividualAchievements(SessionManager.CurrentApplicantId.Value);
                _nextNumber = 1;
                foreach (var ach in achievements)
                {
                    _achievements.Add(new IndividualAchievement
                    {
                        Id = ach.Id,
                        Number = _nextNumber++,
                        Category = ach.Category ?? "",
                        AchievementName = ach.AchievementName ?? ach.Category ?? "",
                        Year = ach.Year ?? "",
                        Points = ach.Points,
                        DocumentName = string.IsNullOrWhiteSpace(ach.DocumentName) ? "Не загружен" : ach.DocumentName,
                        DocumentPath = ach.DocumentPath
                    });
                }
                UpdateTotalPoints();
                Debug.WriteLine($"Загружено достижений: {_achievements.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки достижений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Debug.WriteLine($"LoadAchievementsFromDatabase error: {ex}");
            }
        }

        private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryCombo.SelectedItem is ComboBoxItem item && item.Tag is int points)
            {
                PointsTextBox.Text = points.ToString();
                if (item.Content.ToString() != "Иное")
                    AchievementNameTextBox.Text = item.Content.ToString();
                else
                    AchievementNameTextBox.Text = "";
            }
        }

        private void UploadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл",
                Filter = "PDF файлы (*.pdf)|*.pdf|Изображения (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = Path.GetExtension(dialog.FileName);
                    string uniqueName = $"Achievement_{SessionManager.CurrentApplicantId}_{Guid.NewGuid():N}{extension}";
                    string destPath = Path.Combine(_documentsRootPath, uniqueName);
                    File.Copy(dialog.FileName, destPath, overwrite: false);
                    _uploadedFilePath = destPath;
                    DocumentInfoTextBox.Text = Path.GetFileName(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddAchievementButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(PointsTextBox.Text, out int points) || points < 0)
            {
                MessageBox.Show("Укажите баллы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            string name = string.IsNullOrWhiteSpace(AchievementNameTextBox.Text) ? category : AchievementNameTextBox.Text;
            string year = (YearCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            string docName = string.IsNullOrWhiteSpace(DocumentInfoTextBox.Text) ? "Не загружен" : DocumentInfoTextBox.Text;

            _achievements.Add(new IndividualAchievement
            {
                Id = 0,
                Number = _nextNumber++,
                Category = category,
                AchievementName = name,
                Year = year,
                Points = points,
                DocumentName = docName,
                DocumentPath = _uploadedFilePath
            });

            ClearForm();
            UpdateTotalPoints();
        }

        private void ClearForm()
        {
            CategoryCombo.SelectedIndex = -1;
            AchievementNameTextBox.Text = "";
            YearCombo.SelectedIndex = 0;
            PointsTextBox.Text = "0";
            DocumentInfoTextBox.Text = "";
            _uploadedFilePath = null;
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void DeleteAchievement_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is IndividualAchievement item)
            {
                if (MessageBox.Show($"Удалить \"{item.AchievementName}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _achievements.Remove(item);
                    RenumberAchievements();
                    UpdateTotalPoints();
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as IndividualAchievement;
            if (item != null && !string.IsNullOrEmpty(item.DocumentPath) && File.Exists(item.DocumentPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = item.DocumentPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (item != null && !string.IsNullOrEmpty(item.DocumentName))
            {
                MessageBox.Show($"Файл \"{item.DocumentName}\" не найден на диске.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RenumberAchievements()
        {
            int n = 1;
            foreach (var a in _achievements) a.Number = n++;
            _nextNumber = n;
        }

        private void UpdateTotalPoints() => TotalPointsTextBlock.Text = _achievements.Sum(a => a.Points).ToString();

        private bool SaveData()
        {
            if (isSaving) return false;
            try
            {
                isSaving = true;
                if (SessionManager.CurrentApplicantId == null) return false;

                // Валидация
                foreach (var ach in _achievements)
                {
                    if (string.IsNullOrWhiteSpace(ach.Category))
                    {
                        MessageBox.Show("Выберите категорию для всех достижений.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(ach.AchievementName) || ach.AchievementName.Length < 2)
                    {
                        MessageBox.Show($"Название достижения некорректно: '{ach.AchievementName}'.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(ach.Year))
                    {
                        if (!int.TryParse(ach.Year, out int yearInt) || yearInt < 1900 || yearInt > DateTime.Now.Year)
                        {
                            MessageBox.Show($"Год указан неверно: '{ach.Year}'.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    if (ach.Points < 0)
                    {
                        MessageBox.Show($"Баллы должны быть неотрицательными: {ach.Points}.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                // Удаляем старые записи
                var existing = DatabasePersistenceHelper.LoadIndividualAchievements(SessionManager.CurrentApplicantId.Value);
                foreach (var ach in existing)
                    DatabasePersistenceHelper.DeleteIndividualAchievement(ach.Id, SessionManager.CurrentApplicantId.Value);

                // Сохраняем новые
                foreach (var ach in _achievements)
                {
                    DatabasePersistenceHelper.SaveIndividualAchievement(
                        SessionManager.CurrentApplicantId.Value,
                        ach.Category,
                        ach.AchievementName,
                        ach.Year,
                        ach.Points,
                        ach.DocumentName,
                        ach.DocumentPath,
                        ach.Id > 0 ? ach.Id : (int?)null
                    );
                }

                DataService.LogChange("IndividualAchievements", SessionManager.CurrentApplicantId.Value, "UPDATE");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally { isSaving = false; }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
                NavigationService?.Navigate(new PrioritiesPage());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить изменения?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SessionManager.Clear();
                if (NavigationService?.CanGoBack == true)
                    while (NavigationService.CanGoBack) NavigationService.GoBack();
                else
                    Application.Current.Shutdown();
            }
        }
    }
}