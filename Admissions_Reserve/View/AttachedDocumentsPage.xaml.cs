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
    public partial class AttachedDocumentsPage : Page
    {
        // Модель документа
        public class AttachedDocument : INotifyPropertyChanged
        {
            public int Id { get; set; }
            private int _number;
            private string _documentType;
            private string _seriesNumber;
            private string _category;
            private string _additionalData;
            private DateTime? _issueDate;
            private string _documentInfo;
            private DateTime _addedDate;
            private string _attachmentPath;
            private string _attachmentName;

            public int Number
            {
                get => _number;
                set { _number = value; OnPropertyChanged(nameof(Number)); }
            }
            public string DocumentType
            {
                get => _documentType;
                set { _documentType = value; OnPropertyChanged(nameof(DocumentType)); }
            }
            public string SeriesNumber
            {
                get => _seriesNumber;
                set { _seriesNumber = value; OnPropertyChanged(nameof(SeriesNumber)); }
            }
            public string Category
            {
                get => _category;
                set { _category = value; OnPropertyChanged(nameof(Category)); }
            }
            public string AdditionalData
            {
                get => _additionalData;
                set { _additionalData = value; OnPropertyChanged(nameof(AdditionalData)); }
            }
            public DateTime? IssueDate
            {
                get => _issueDate;
                set { _issueDate = value; OnPropertyChanged(nameof(IssueDate)); }
            }
            public string DocumentInfo
            {
                get => _documentInfo;
                set { _documentInfo = value; OnPropertyChanged(nameof(DocumentInfo)); }
            }
            public DateTime AddedDate
            {
                get => _addedDate;
                set { _addedDate = value; OnPropertyChanged(nameof(AddedDate)); }
            }
            public string AttachmentPath
            {
                get => _attachmentPath;
                set { _attachmentPath = value; OnPropertyChanged(nameof(AttachmentPath)); }
            }
            public string AttachmentName
            {
                get => _attachmentName;
                set { _attachmentName = value; OnPropertyChanged(nameof(AttachmentName)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<AttachedDocument> _documents;
        private int _nextNumber = 1;
        private string _selectedAttachmentPath;
        private string _selectedAttachmentName;

        public AttachedDocumentsPage()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            _documents = new ObservableCollection<AttachedDocument>();

            // Загружаем данные из БД если есть абитуриент
            if (SessionManager.CurrentApplicant != null)
            {
                LoadDocumentsFromDatabase();
            }
            else
            {
                LoadSampleDocuments();
            }

            DocumentsGrid.ItemsSource = _documents;
        }

        private void LoadDocumentsFromDatabase()
        {
            try
            {
                var documents = DatabasePersistenceHelper.LoadAttachedDocuments(SessionManager.CurrentApplicantId.Value);
                foreach (var doc in documents)
                {
                    _documents.Add(new AttachedDocument
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = doc.DocumentType,
                        SeriesNumber = doc.SeriesNumber,
                        Category = doc.Category,
                        AdditionalData = doc.AdditionalData,
                        IssueDate = doc.IssueDate,
                        DocumentInfo = doc.DocumentInfo,
                        AddedDate = doc.CreatedAt,
                        AttachmentName = doc.AttachmentName,
                        AttachmentPath = doc.AttachmentPath
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoadSampleDocuments();
            }
        }

        private void LoadSampleDocuments()
        {
            // Пустой список для новых абитуриентов
        }

        private void UploadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл документа",
                Filter = "PDF файлы (*.pdf)|*.pdf|Изображения (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|Документы (*.doc;*.docx)|*.doc;*.docx|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedAttachmentPath = openFileDialog.FileName;
                _selectedAttachmentName = System.IO.Path.GetFileName(_selectedAttachmentPath);
                AttachmentFileNameTextBox.Text = _selectedAttachmentName;
            }
        }

        private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(DocumentTypeCombo.Text))
            {
                MessageBox.Show("Пожалуйста, укажите название документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int documentId = 0;
                if (SessionManager.CurrentApplicant != null)
                {
                    documentId = DatabasePersistenceHelper.SaveAttachedDocument(
                        SessionManager.CurrentApplicantId.Value,
                        DocumentTypeCombo.Text,
                        SeriesNumberTextBox.Text?.Trim(),
                        (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        AdditionalDataTextBox.Text?.Trim(),
                        IssueDatePicker.SelectedDate,
                        DocumentInfoTextBox.Text?.Trim(),
                        _selectedAttachmentPath,
                        _selectedAttachmentName
                    );
                    DataService.LogChange("AttachedDocuments", documentId, "INSERT");
                }

                _documents.Add(new AttachedDocument
                {
                    Id = documentId,
                    Number = _nextNumber++,
                    DocumentType = DocumentTypeCombo.Text,
                    SeriesNumber = SeriesNumberTextBox.Text,
                    Category = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    AdditionalData = AdditionalDataTextBox.Text,
                    IssueDate = IssueDatePicker.SelectedDate,
                    DocumentInfo = DocumentInfoTextBox.Text,
                    AddedDate = DateTime.Now,
                    AttachmentName = _selectedAttachmentName,
                    AttachmentPath = _selectedAttachmentPath
                });

                ClearForm();

                MessageBox.Show("Документ успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении документа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            DocumentTypeCombo.Text = "";
            SeriesNumberTextBox.Text = "";
            CategoryCombo.SelectedIndex = 0;
            AdditionalDataTextBox.Text = "";
            IssueDatePicker.SelectedDate = null;
            DocumentInfoTextBox.Text = "";
            AttachmentFileNameTextBox.Text = "";
            _selectedAttachmentPath = null;
            _selectedAttachmentName = null;
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as AttachedDocument;

            if (item != null)
            {
                var result = MessageBox.Show($"Удалить документ \"{item.DocumentType}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (item.Id > 0 && SessionManager.CurrentApplicant != null)
                        {
                            DatabasePersistenceHelper.DeleteAttachedDocument(item.Id, SessionManager.CurrentApplicantId.Value);
                            DataService.LogChange("AttachedDocuments", item.Id, "DELETE");
                        }

                        _documents.Remove(item);
                        RenumberDocuments();

                        MessageBox.Show("Документ удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as AttachedDocument;

            if (item != null && !string.IsNullOrEmpty(item.AttachmentPath))
            {
                try
                {
                    // Открываем диалог сохранения файла
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Title = "Сохранить файл",
                        FileName = item.AttachmentName,
                        Filter = "Все файлы (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        System.IO.File.Copy(item.AttachmentPath, saveFileDialog.FileName, true);
                        MessageBox.Show("Файл успешно сохранен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (item != null && string.IsNullOrEmpty(item.AttachmentPath))
            {
                MessageBox.Show("Файл не прикреплен к документу", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RenumberDocuments()
        {
            int number = 1;
            foreach (var doc in _documents)
            {
                doc.Number = number++;
            }
            _nextNumber = number;
        }

        // Сохранение данных в БД
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

                // Данные сохраняются при добавлении каждого документа
                MessageBox.Show("Данные о прикрепленных документах уже сохранены!", "Успех",
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

        // Кнопка ДАЛЕЕ - переход на страницу дополнительной информации
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
                    NavigationService?.Navigate(new AdditionalInfoPage());
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

        // Кнопка НАЗАД - возврат на страницу приоритетов
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
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
}