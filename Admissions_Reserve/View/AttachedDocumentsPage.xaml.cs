using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class AttachedDocumentsPage : Page
    {
        public class AttachedDocument : INotifyPropertyChanged
        {
            private int _id;
            private int _number;
            private string _documentType;
            private string _seriesNumber;
            private string _category;
            private DateTime? _issueDate;
            private string _documentInfo;
            private string _sourceTable;

            public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
            public int Number { get => _number; set { _number = value; OnPropertyChanged(nameof(Number)); } }
            public string DocumentType { get => _documentType; set { _documentType = value; OnPropertyChanged(nameof(DocumentType)); } }
            public string SeriesNumber { get => _seriesNumber; set { _seriesNumber = value; OnPropertyChanged(nameof(SeriesNumber)); } }
            public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }
            public DateTime? IssueDate { get => _issueDate; set { _issueDate = value; OnPropertyChanged(nameof(IssueDate)); } }
            public string DocumentInfo { get => _documentInfo; set { _documentInfo = value; OnPropertyChanged(nameof(DocumentInfo)); } }
            public string SourceTable { get => _sourceTable; set { _sourceTable = value; OnPropertyChanged(nameof(SourceTable)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<AttachedDocument> _documents;
        private int _nextNumber = 1;

        public AttachedDocumentsPage()
        {
            InitializeComponent();
            _documents = new ObservableCollection<AttachedDocument>();
            DocumentsGrid.ItemsSource = _documents;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int? applicantId = SessionManager.CurrentApplicantId;
            if (applicantId == null || applicantId == 0)
            {
                MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                return;
            }

            LoadAllDocuments(applicantId.Value);
        }

        private void LoadAllDocuments(int applicantId)
        {
            try
            {
                _documents.Clear();
                _nextNumber = 1;

                // 1. Документ об образовании
                var eduDocs = DataService.GetApplicantEducationDocuments(applicantId);
                foreach (var eduDoc in eduDocs)
                {
                    Debug.WriteLine($"Образование: Id={eduDoc.Id}, Серия={eduDoc.Series}, Номер={eduDoc.Number}");
                    _documents.Add(new AttachedDocument
                    {
                        Id = eduDoc.Id,
                        Number = _nextNumber++,
                        DocumentType = GetEducationDocumentTypeName(eduDoc.DocumentTypeId) ?? "Документ об образовании",
                        SeriesNumber = FormatSeriesNumber(eduDoc.Series, eduDoc.Number),
                        Category = "Документ об образовании",
                        IssueDate = eduDoc.IssueDate,
                        DocumentInfo = $"{eduDoc.EducationalOrg}, {eduDoc.City}".TrimStart(',').Trim(),
                        SourceTable = "EducationDocuments"
                    });
                }

                // 2. Удостоверения личности
                var idDocs = DataService.GetAllIdentityDocuments(applicantId);
                foreach (var idDoc in idDocs)
                {
                    Debug.WriteLine($"Удостоверение: Id={idDoc.Id}, Серия={idDoc.Series}, Номер={idDoc.Number}");
                    var docType = GetIdentityDocumentTypeName(idDoc.DocumentTypeId);
                    _documents.Add(new AttachedDocument
                    {
                        Id = idDoc.Id,
                        Number = _nextNumber++,
                        DocumentType = docType ?? "Удостоверение личности",
                        SeriesNumber = FormatSeriesNumber(idDoc.Series, idDoc.Number),
                        Category = "Удостоверение личности",
                        IssueDate = idDoc.IssueDate,
                        DocumentInfo = idDoc.IssuedBy ?? "",
                        SourceTable = "IdentityDocuments"
                    });
                }

                // 3. Обычные документы (таблица Documents)
                var generalDocs = DataService.GetAllGeneralDocuments(applicantId);
                foreach (var doc in generalDocs)
                {
                    var docTypeName = GetPersonalDocumentTypeName(doc.DocumentTypeId);
                    _documents.Add(new AttachedDocument
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = docTypeName ?? "Документ",
                        SeriesNumber = FormatSeriesNumber(doc.Series, doc.Number),
                        Category = doc.Category ?? "Абитуриент",
                        IssueDate = doc.IssueDate,
                        DocumentInfo = doc.DocumentInfo ?? "",
                        SourceTable = "Documents"
                    });
                }

                // 4. Прикреплённые файлы (если таблица существует)
                try
                {
                    var attachedDocs = DataService.GetApplicantAttachedDocuments(applicantId);
                    foreach (var doc in attachedDocs)
                    {
                        _documents.Add(new AttachedDocument
                        {
                            Id = doc.Id,
                            Number = _nextNumber++,
                            DocumentType = doc.DocumentName ?? doc.DocumentType ?? "Приложение",
                            SeriesNumber = "",
                            Category = "Прикреплённый файл",
                            IssueDate = doc.UploadedAt,
                            DocumentInfo = doc.FilePath,
                            SourceTable = "AttachedDocuments"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Таблица AttachedDocuments отсутствует: {ex.Message}");
                }

                Debug.WriteLine($"Всего загружено документов: {_documents.Count}");
                DocumentsGrid.Items.Refresh();

                if (_documents.Count == 0)
                {
                    MessageBox.Show("Нет добавленных документов. Пожалуйста, вернитесь и добавьте необходимые документы.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatSeriesNumber(string series, string number)
        {
            series = series?.Trim() ?? "";
            number = number?.Trim() ?? "";
            if (!string.IsNullOrEmpty(series) && !string.IsNullOrEmpty(number))
                return $"{series} {number}";
            if (!string.IsNullOrEmpty(number))
                return number;
            if (!string.IsNullOrEmpty(series))
                return series;
            return "";
        }

        private string GetEducationDocumentTypeName(int? id)
        {
            if (id == null) return null;
            try { return DataService.GetAll<EducationDocumentTypes>().FirstOrDefault(t => t.Id == id)?.Name; }
            catch { return null; }
        }

        private string GetIdentityDocumentTypeName(int? id)
        {
            if (id == null) return null;
            try { return DataService.GetAll<IdentityDocumentTypes>().FirstOrDefault(t => t.Id == id)?.Name; }
            catch { return null; }
        }

        private string GetPersonalDocumentTypeName(int? id)
        {
            if (id == null) return null;
            try { return DataService.GetAll<PersonalDocumentTypes>().FirstOrDefault(t => t.Id == id)?.Name; }
            catch { return null; }
        }

        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is AttachedDocument item)
            {
                if (MessageBox.Show($"Удалить \"{item.DocumentType}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                try
                {
                    switch (item.SourceTable)
                    {
                        case "EducationDocuments":
                            DataService.DeleteEducationDocument(item.Id);
                            break;
                        case "IdentityDocuments":
                            DataService.DeleteIdentityDocument(item.Id);
                            break;
                        case "Documents":
                            DataService.DeleteGeneralDocument(item.Id, SessionManager.CurrentApplicantId.Value);
                            break;
                        case "AttachedDocuments":
                            DataService.DeleteAttachedDocument(item.Id);
                            break;
                    }
                    DataService.LogChange(item.SourceTable, item.Id, "DELETE");
                    _documents.Remove(item);
                    RenumberDocuments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenumberDocuments()
        {
            int n = 1;
            foreach (var d in _documents) d.Number = n++;
            _nextNumber = n;
            DocumentsGrid.Items.Refresh();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? applicantId = SessionManager.CurrentApplicantId;
                if (applicantId != null && applicantId != 0)
                {
                    var applicant = DataService.GetApplicant(applicantId.Value);
                    if (applicant != null)
                    {
                        applicant.UpdatedAt = DateTime.Now;
                        DataService.UpdateApplicant(applicant);
                    }
                    DataService.LogChange("Applicants", applicantId.Value, "COMPLETE");
                }

                MessageBox.Show("Заявление успешно заполнено! Все данные сохранены.\nВы будете перенаправлены на страницу поиска абитуриентов.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                SessionManager.Clear();

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                    mainWindow.MainFrame.Navigate(new ApplicantSearchPage());
                else
                    NavigationService?.Navigate(new ApplicantSearchPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Вы уверены, что хотите отменить ввод данных?\n\n" +
                "ВНИМАНИЕ: Все данные абитуриента будут безвозвратно удалены из базы данных!\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            if (MessageBox.Show("Вы действительно хотите удалить все данные этого абитуриента?",
                "Подтвердите удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                int? applicantId = SessionManager.CurrentApplicantId;
                if (applicantId != null && applicantId != 0)
                {
                    DataService.DeleteApplicant(applicantId.Value);
                    DataService.LogChange("Applicants", applicantId.Value, "DELETE_ALL");
                }

                SessionManager.Clear();
                MessageBox.Show("Все данные абитуриента удалены.\nВы будете перенаправлены на страницу поиска.",
                    "Удалено", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new ApplicantSearchPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}