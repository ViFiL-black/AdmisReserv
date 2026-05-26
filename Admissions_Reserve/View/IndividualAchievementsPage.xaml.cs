using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            public int Id { get; set; }
            public int Number { get; set; }
            public string Category { get; set; }
            public string AchievementName { get; set; }
            public string Year { get; set; }
            public int Points { get; set; }
            public string DocumentName { get; set; }
            public string DocumentPath { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<IndividualAchievement> _achievements;
        private string _uploadedFilePath;
        private int _nextNumber = 1;
        private bool isSaving = false;

        public IndividualAchievementsPage()
        {
            InitializeComponent();
            _achievements = new ObservableCollection<IndividualAchievement>();
            AchievementsGrid.ItemsSource = _achievements;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
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
                // Если таблицы нет, используем значения по умолчанию
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
                // Use DatabasePersistenceHelper which returns detailed records
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var dialog = new OpenFileDialog { Title = "Выберите файл", Filter = "Все файлы (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                _uploadedFilePath = dialog.FileName;
                DocumentInfoTextBox.Text = System.IO.Path.GetFileName(_uploadedFilePath);
            }
        }

        private void AddAchievementButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryCombo.SelectedItem == null)
            { MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(PointsTextBox.Text, out int points) || points < 0)
            { MessageBox.Show("Укажите баллы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            string category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            string name = string.IsNullOrWhiteSpace(AchievementNameTextBox.Text) ? category : AchievementNameTextBox.Text;
            string year = (YearCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            string doc = string.IsNullOrWhiteSpace(DocumentInfoTextBox.Text) ? "Не загружен" : DocumentInfoTextBox.Text;

            _achievements.Add(new IndividualAchievement
            {
                Number = _nextNumber++,
                Category = category,
                AchievementName = name,
                Year = year,
                Points = points,
                DocumentName = doc,
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
                { _achievements.Remove(item); RenumberAchievements(); UpdateTotalPoints(); }
            }
        }

        private void RenumberAchievements() { int n = 1; foreach (var a in _achievements) a.Number = n++; _nextNumber = n; }

        private void UpdateTotalPoints() => TotalPointsTextBlock.Text = _achievements.Sum(a => a.Points).ToString();

        private bool SaveData()
        {
            if (isSaving) return false;
            try
            {
                isSaving = true;
                if (SessionManager.CurrentApplicantId == null) return false;

                // Валидация каждой записи
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
                        if (!int.TryParse(ach.Year, out int yearInt) || !ValidationHelper.IsValidYear(yearInt))
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

                    if (!string.IsNullOrWhiteSpace(ach.DocumentPath))
                    {
                        try
                        {
                            if (!System.IO.File.Exists(ach.DocumentPath))
                            {
                                MessageBox.Show($"Файл подтверждения достижения не найден: {ach.DocumentPath}", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }
                        catch { }
                    }
                }

                var existing = DataService.GetApplicantIndividualAchievements(SessionManager.CurrentApplicantId.Value);
                foreach (var ach in existing) DataService.DeleteIndividualAchievement(ach.Id);

                foreach (var ach in _achievements)
                {
                    DataService.CreateIndividualAchievementFull(SessionManager.CurrentApplicantId.Value, ach.Category, ach.AchievementName, ach.Year, ach.Points, ach.DocumentName, ach.DocumentPath);
                }

                DataService.LogChange("IndividualAchievements", SessionManager.CurrentApplicantId.Value, "UPDATE");
                return true;
            }
            catch (Exception ex)
            { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
            finally { isSaving = false; }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e) { if (NavigationService?.CanGoBack == true) NavigationService.GoBack(); }
        private void NextButton_Click(object sender, RoutedEventArgs e) { if (SaveData()) MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { SessionManager.Clear(); if (NavigationService?.CanGoBack == true) while (NavigationService.CanGoBack) NavigationService.GoBack(); else Application.Current.Shutdown(); }
        }
    }
}