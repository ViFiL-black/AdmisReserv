using Admissions_Reserve.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Admissions_Reserve.View
{
    public partial class DocumentsPage : Page
    {
        // Конвертер видимости для файлов
        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return (value is bool boolValue && boolValue) ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value is Visibility visibility && visibility == Visibility.Visible;
            }
        }

        // Модель документа
        public class DocumentItem : INotifyPropertyChanged
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
            private string _personalDataCategory;
            private bool _isPersonalDataDocument;
            private string _attachmentPath;
            private string _attachmentName;
            private bool _hasAttachment;
            private int? _documentTypeId;

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
            public string PersonalDataCategory
            {
                get => _personalDataCategory;
                set { _personalDataCategory = value; OnPropertyChanged(nameof(PersonalDataCategory)); }
            }
            public bool IsPersonalDataDocument
            {
                get => _isPersonalDataDocument;
                set { _isPersonalDataDocument = value; OnPropertyChanged(nameof(IsPersonalDataDocument)); }
            }
            public string AttachmentPath
            {
                get => _attachmentPath;
                set
                {
                    _attachmentPath = value;
                    OnPropertyChanged(nameof(AttachmentPath));
                    HasAttachment = !string.IsNullOrEmpty(value);
                }
            }
            public string AttachmentName
            {
                get => _attachmentName;
                set { _attachmentName = value; OnPropertyChanged(nameof(AttachmentName)); }
            }
            public bool HasAttachment
            {
                get => _hasAttachment;
                set { _hasAttachment = value; OnPropertyChanged(nameof(HasAttachment)); }
            }
            public int? DocumentTypeId
            {
                get => _documentTypeId;
                set { _documentTypeId = value; OnPropertyChanged(nameof(DocumentTypeId)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<DocumentItem> _documents;
        private ObservableCollection<DocumentItem> _personalDataDocuments;
        private int _nextNumber = 1;
        private string _selectedAttachmentPath;
        private string _selectedAttachmentName;
        private bool isInitialized = false;
        private bool isNavigating = false;
        private bool _isLoadingData = false;

        // Коллекции для ComboBox
        private ObservableCollection<IdentityDocumentTypes> _identityDocumentTypes;
        private ObservableCollection<PersonalDocumentTypes> _personalDocumentTypes;

        public DocumentsPage()
        {
            InitializeComponent();
            Loaded += DocumentsPage_Loaded;
        }

        private void DocumentsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Resources.Add("BoolToVisibility", new BoolToVisibilityConverter());

            InitializeData();
            LoadDocumentTypesFromDatabase();
            LoadDocumentsFromDatabase();

            isInitialized = true;
        }

        private void InitializeData()
        {
            _documents = new ObservableCollection<DocumentItem>();
            _personalDataDocuments = new ObservableCollection<DocumentItem>();
            _identityDocumentTypes = new ObservableCollection<IdentityDocumentTypes>();
            _personalDocumentTypes = new ObservableCollection<PersonalDocumentTypes>();

            DocumentsGrid.ItemsSource = _documents;
            PersonalDataDocumentsGrid.ItemsSource = _personalDataDocuments;
        }

        /// <summary>
        /// Загрузка типов документов из базы данных
        /// </summary>
        private void LoadDocumentTypesFromDatabase()
        {
            try
            {
                // Загружаем типы документов, удостоверяющих личность
                var identityTypes = DataService.GetAll<IdentityDocumentTypes>();
                _identityDocumentTypes.Clear();
                foreach (var type in identityTypes)
                {
                    _identityDocumentTypes.Add(type);
                }

                // Загружаем типы персональных документов из БД
                var personalTypes = DataService.GetAll<PersonalDocumentTypes>();
                _personalDocumentTypes.Clear();
                foreach (var type in personalTypes)
                {
                    _personalDocumentTypes.Add(type);
                }

                // Если таблица пуста, добавляем базовые типы
                if (_personalDocumentTypes.Count == 0)
                {
                    AddDefaultPersonalDocumentTypes();
                }

                // Устанавливаем источник для ComboBox
                UpdateDocumentTypeCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // В случае ошибки создаем базовые типы программно
                AddDefaultPersonalDocumentTypes();
                UpdateDocumentTypeCombo();
            }
        }

        private void AddDefaultPersonalDocumentTypes()
        {
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 1, Name = "Медицинская справка 086у" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 2, Name = "Полис ОМС" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 3, Name = "СНИЛС" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 4, Name = "ИНН" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 5, Name = "Фото 3x4" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 6, Name = "Признанный сертификат" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 7, Name = "Справка об инвалидности" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 8, Name = "Свидетельство о браке" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 9, Name = "Свидетельство о рождении" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 10, Name = "Военный билет" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 11, Name = "Водительское удостоверение" });
            _personalDocumentTypes.Add(new PersonalDocumentTypes { Id = 12, Name = "Загранпаспорт" });
        }

        private void UpdateDocumentTypeCombo()
        {
            if (IdentityDocRadio.IsChecked == true)
            {
                DocumentTypeCombo.ItemsSource = _identityDocumentTypes;
                DocumentTypeCombo.DisplayMemberPath = "Name";
                DocumentTypeCombo.SelectedValuePath = "Id";
                if (_identityDocumentTypes.Count > 0)
                    DocumentTypeCombo.SelectedIndex = 0;
            }
            else
            {
                DocumentTypeCombo.ItemsSource = _personalDocumentTypes;
                DocumentTypeCombo.DisplayMemberPath = "Name";
                DocumentTypeCombo.SelectedValuePath = "Id";
                if (_personalDocumentTypes.Count > 0)
                    DocumentTypeCombo.SelectedIndex = 0;
            }
        }

        private void LoadDocumentsFromDatabase()
        {
            if (_isLoadingData) return;
            _isLoadingData = true;

            try
            {
                if (SessionManager.CurrentApplicantId == null) return;

                _nextNumber = 1;
                _documents.Clear();
                _personalDataDocuments.Clear();

                // Загружаем документы, удостоверяющие личность
                var identityDocs = DataService.GetApplicantDocuments(SessionManager.CurrentApplicantId.Value);
                foreach (var doc in identityDocs)
                {
                    var docType = GetIdentityDocumentTypeName(doc.DocumentTypeId);
                    var seriesNumber = $"{doc.Series} {doc.Number}".Trim();

                    _personalDataDocuments.Add(new DocumentItem
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = docType,
                        DocumentTypeId = doc.DocumentTypeId,
                        SeriesNumber = seriesNumber,
                        Category = "Абитуриент (прием 2026)",
                        AdditionalData = "",
                        IssueDate = doc.IssueDate,
                        DocumentInfo = doc.IssuedBy,
                        AddedDate = doc.AddedDate ?? DateTime.Now,
                        PersonalDataCategory = "Абитуриент (прием 2026)",
                        IsPersonalDataDocument = true,
                        HasAttachment = false
                    });
                }

                // Загружаем общие документы
                var generalDocs = DataService.GetAllGeneralDocuments(SessionManager.CurrentApplicantId.Value);
                foreach (var doc in generalDocs)
                {
                    var seriesNumber = $"{doc.Series} {doc.Number}".Trim();
                    var docType = GetPersonalDocumentTypeName(doc.DocumentTypeId);

                    _documents.Add(new DocumentItem
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = docType,
                        DocumentTypeId = doc.DocumentTypeId,
                        SeriesNumber = seriesNumber,
                        Category = "Абитуриент (прием 2026)",
                        AdditionalData = "",
                        IssueDate = null,
                        DocumentInfo = "",
                        AddedDate = DateTime.Now,
                        PersonalDataCategory = "Абитуриент (прием 2026)",
                        IsPersonalDataDocument = false,
                        HasAttachment = false
                    });
                }

                RenumberItems(_personalDataDocuments);
                RenumberItems(_documents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        private string GetIdentityDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return "Документ";
            try
            {
                var type = _identityDocumentTypes.FirstOrDefault(dt => dt.Id == documentTypeId.Value);
                return type?.Name ?? "Документ";
            }
            catch
            {
                return "Документ";
            }
        }

        private string GetPersonalDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return "Документ";
            try
            {
                var type = _personalDocumentTypes.FirstOrDefault(dt => dt.Id == documentTypeId.Value);
                return type?.Name ?? "Документ";
            }
            catch
            {
                return "Документ";
            }
        }

        private void IdentityDocRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            UpdateDocumentTypeCombo();
        }

        private void OtherDocRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            UpdateDocumentTypeCombo();
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
                _selectedAttachmentName = Path.GetFileName(_selectedAttachmentPath);
                AttachmentFileTextBox.Text = _selectedAttachmentName;
            }
        }

        private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (DocumentTypeCombo.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DocumentTypeCombo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, укажите номер документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NumberTextBox.Focus();
                return;
            }

            try
            {
                if (SessionManager.CurrentApplicantId == null)
                {
                    MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем выбранный тип документа
                int selectedTypeId = (int)DocumentTypeCombo.SelectedValue;
                string selectedTypeName = DocumentTypeCombo.Text;

                // Формируем серию и номер
                string series = SeriesTextBox.Text?.Trim() ?? "";
                string number = NumberTextBox.Text?.Trim() ?? "";
                string seriesNumber = string.IsNullOrEmpty(series) ? number : $"{series} {number}";

                // Определяем категорию персональных данных
                string personalDataCategory = (CategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Абитуриент (прием 2026)";

                // Определяем, является ли документ удостоверением личности
                bool isIdentityDocument = IdentityDocRadio.IsChecked == true;

                // Формируем информацию о документе
                string documentInfo = DocumentInfoTextBox.Text;
                if (string.IsNullOrWhiteSpace(documentInfo) && isIdentityDocument)
                {
                    documentInfo = selectedTypeName;
                    if (!string.IsNullOrWhiteSpace(IssuedByTextBox.Text))
                        documentInfo += $", выдан {IssuedByTextBox.Text}";
                }

                int documentId = 0;

                if (isIdentityDocument)
                {
                    // Создаем документ, удостоверяющий личность
                    var newDoc = new IdentityDocuments
                    {
                        ApplicantId = SessionManager.CurrentApplicantId.Value,
                        DocumentTypeId = selectedTypeId,
                        Series = series,
                        Number = number,
                        IssuedBy = IssuedByTextBox.Text?.Trim(),
                        IssueDate = IssueDatePicker.SelectedDate,
                        DepartmentCode = "",
                        IsPrimary = _personalDataDocuments.Count == 0,
                        AddedDate = DateTime.Now
                    };

                    documentId = DataService.CreateIdentityDocument(newDoc);
                    DataService.LogChange("IdentityDocuments", documentId, "INSERT");

                    var newDocument = new DocumentItem
                    {
                        Id = documentId,
                        Number = _nextNumber++,
                        DocumentType = selectedTypeName,
                        DocumentTypeId = selectedTypeId,
                        SeriesNumber = seriesNumber,
                        Category = personalDataCategory,
                        AdditionalData = AdditionalDataTextBox.Text,
                        IssueDate = IssueDatePicker.SelectedDate,
                        DocumentInfo = documentInfo,
                        AddedDate = DateTime.Now,
                        PersonalDataCategory = personalDataCategory,
                        IsPersonalDataDocument = true,
                        AttachmentPath = _selectedAttachmentPath,
                        AttachmentName = _selectedAttachmentName,
                        HasAttachment = !string.IsNullOrEmpty(_selectedAttachmentPath)
                    };

                    _personalDataDocuments.Add(newDocument);
                    RenumberItems(_personalDataDocuments);
                    MessageBox.Show("Документ, удостоверяющий личность, успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Создаем общий документ (PersonalDocumentTypes)
                    var newDoc = new Documents
                    {
                        ApplicantId = SessionManager.CurrentApplicantId.Value,
                        DocumentTypeId = selectedTypeId,
                        Series = series,
                        Number = number
                    };

                    documentId = DataService.CreateGeneralDocument(newDoc);
                    DataService.LogChange("Documents", documentId, "INSERT");

                    var newDocument = new DocumentItem
                    {
                        Id = documentId,
                        Number = _nextNumber++,
                        DocumentType = selectedTypeName,
                        DocumentTypeId = selectedTypeId,
                        SeriesNumber = seriesNumber,
                        Category = personalDataCategory,
                        AdditionalData = AdditionalDataTextBox.Text,
                        IssueDate = IssueDatePicker.SelectedDate,
                        DocumentInfo = documentInfo,
                        AddedDate = DateTime.Now,
                        PersonalDataCategory = personalDataCategory,
                        IsPersonalDataDocument = false,
                        AttachmentPath = _selectedAttachmentPath,
                        AttachmentName = _selectedAttachmentName,
                        HasAttachment = !string.IsNullOrEmpty(_selectedAttachmentPath)
                    };

                    _documents.Add(newDocument);
                    RenumberItems(_documents);
                    MessageBox.Show("Документ успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении документа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            if (DocumentTypeCombo.Items.Count > 0)
                DocumentTypeCombo.SelectedIndex = 0;
            SeriesTextBox.Text = "";
            NumberTextBox.Text = "";
            IssueDatePicker.SelectedDate = null;
            IssuedByTextBox.Text = "";
            AdditionalDataTextBox.Text = "";
            DocumentInfoTextBox.Text = "";
            AttachmentFileTextBox.Text = "";
            _selectedAttachmentPath = null;
            _selectedAttachmentName = null;

            IdentityDocRadio.IsChecked = true;
            CategoryCombo.SelectedIndex = 0;
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as DocumentItem;

            if (item != null)
            {
                var result = MessageBox.Show($"Удалить документ \"{item.DocumentType}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (item.Id > 0 && SessionManager.CurrentApplicantId.HasValue)
                        {
                            if (item.IsPersonalDataDocument)
                            {
                                using (var connection = DatabaseHelper.GetConnection())
                                {
                                    var query = "DELETE FROM IdentityDocuments WHERE Id = @Id AND ApplicantId = @ApplicantId";
                                    using (var cmd = new SQLiteCommand(query, connection))
                                    {
                                        cmd.Parameters.AddWithValue("@Id", item.Id);
                                        cmd.Parameters.AddWithValue("@ApplicantId", SessionManager.CurrentApplicantId.Value);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                DataService.LogChange("IdentityDocuments", item.Id, "DELETE");
                            }
                            else
                            {
                                DataService.DeleteGeneralDocument(item.Id, SessionManager.CurrentApplicantId.Value);
                                DataService.LogChange("Documents", item.Id, "DELETE");
                            }
                        }

                        if (item.IsPersonalDataDocument)
                        {
                            _personalDataDocuments.Remove(item);
                            RenumberItems(_personalDataDocuments);
                        }
                        else
                        {
                            _documents.Remove(item);
                            RenumberItems(_documents);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as DocumentItem;

            if (item != null && !string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = item.AttachmentPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (item != null && !string.IsNullOrEmpty(item.AttachmentName))
            {
                MessageBox.Show($"Файл \"{item.AttachmentName}\" не найден на диске", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RenumberItems(ObservableCollection<DocumentItem> items)
        {
            int number = 1;
            foreach (var item in items)
            {
                item.Number = number++;
            }
        }

        public bool ValidateDocuments()
        {
            if (_personalDataDocuments.Count == 0)
            {
                MessageBox.Show("Необходимо добавить хотя бы один документ, удостоверяющий личность",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool SaveData()
        {
            return ValidateDocuments();
        }

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
                    NavigationService?.Navigate(new RelativesPage());
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

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNavigating) return;

            SaveData();
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
            {
                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

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