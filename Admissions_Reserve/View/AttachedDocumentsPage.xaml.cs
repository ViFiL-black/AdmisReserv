using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public partial class AttachedDocumentsPage : Page
    {
        // Модель документа для отображения в таблице
        public class AttachedDocument : INotifyPropertyChanged
        {
            private int _id;
            private int _number;
            private string _documentType;
            private string _seriesNumber;
            private string _category;
            private DateTime? _issueDate;
            private string _documentInfo;
            private string _sourceTable; // откуда взят документ (для удаления)

            public int Id
            {
                get => _id;
                set { _id = value; OnPropertyChanged(nameof(Id)); }
            }
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
            public string SourceTable
            {
                get => _sourceTable;
                set { _sourceTable = value; OnPropertyChanged(nameof(SourceTable)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
            int? applicantId = SessionManager.CurrentApplicantId ??
                (SessionManager.CurrentApplicant?.Id != 0 ? (int?)SessionManager.CurrentApplicant?.Id : null);

            if (applicantId == null)
            {
                MessageBox.Show("Сначала необходимо заполнить данные удостоверения личности",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                return;
            }

            LoadAllDocuments(applicantId.Value);
        }

        /// <summary>
        /// Загружает все документы абитуриента из разных таблиц и объединяет в одну таблицу
        /// </summary>
        private void LoadAllDocuments(int applicantId)
        {
            try
            {
                _documents.Clear();
                _nextNumber = 1;

                // 1. Документ об образовании
                var eduDocs = DataService.GetApplicantEducationDocuments(applicantId);
                var eduDoc = eduDocs.FirstOrDefault();
                if (eduDoc != null)
                {
                    _documents.Add(new AttachedDocument
                    {
                        Id = eduDoc.Id,
                        Number = _nextNumber++,
                        DocumentType = GetEducationDocumentTypeName(eduDoc.DocumentTypeId) ?? "Документ об образовании",
                        SeriesNumber = $"{eduDoc.Series} {eduDoc.Number}".Trim(),
                        Category = "Документ об образовании",
                        IssueDate = eduDoc.IssueDate,
                        DocumentInfo = $"{eduDoc.EducationalOrg}, {eduDoc.City}",
                        SourceTable = "EducationDocuments"
                    });
                }

                // 2. Удостоверение личности (только основной, если есть, иначе первый)
                var idDocs = DataService.GetAllIdentityDocuments(applicantId);
                var idDoc = idDocs.FirstOrDefault(d => d.IsPrimary == true) ?? idDocs.FirstOrDefault();
                if (idDoc != null)
                {
                    var docType = GetIdentityDocumentTypeName(idDoc.DocumentTypeId);
                    _documents.Add(new AttachedDocument
                    {
                        Id = idDoc.Id,
                        Number = _nextNumber++,
                        DocumentType = docType ?? "Удостоверение личности",
                        SeriesNumber = $"{idDoc.Series} {idDoc.Number}".Trim(),
                        Category = "Удостоверение личности",
                        IssueDate = idDoc.IssueDate,
                        DocumentInfo = idDoc.IssuedBy,
                        SourceTable = "IdentityDocuments"
                    });
                }

                // 3. Обычные документы (из таблицы Documents, добавленные на странице DocumentsPage)
                var generalDocs = DataService.GetAllGeneralDocuments(applicantId);
                foreach (var doc in generalDocs)
                {
                    var docTypeName = GetPersonalDocumentTypeName(doc.DocumentTypeId);
                    _documents.Add(new AttachedDocument
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = docTypeName ?? "Документ",
                        SeriesNumber = $"{doc.Series} {doc.Number}".Trim(),
                        Category = doc.Category ?? "Абитуриент",
                        IssueDate = doc.IssueDate,
                        DocumentInfo = doc.DocumentInfo,
                        SourceTable = "Documents"
                    });
                }

                // 4. Прикреплённые файлы (таблица AttachedDocuments)
                var attachedDocs = DataService.GetApplicantAttachedDocuments(applicantId);
                foreach (var doc in attachedDocs)
                {
                    _documents.Add(new AttachedDocument
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = doc.DocumentName ?? doc.DocumentType ?? "Приложение",
                        SeriesNumber = "",  // у файлов обычно нет серии/номера
                        Category = "Прикреплённый файл",
                        IssueDate = doc.UploadedAt,
                        DocumentInfo = doc.FilePath,
                        SourceTable = "AttachedDocuments"
                    });
                }

                // Обновляем отображение
                DocumentsGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательные методы для получения названий типов документов
        private string GetEducationDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return null;
            try
            {
                var types = DataService.GetAll<EducationDocumentTypes>();
                return types.FirstOrDefault(t => t.Id == documentTypeId)?.Name;
            }
            catch { return null; }
        }

        private string GetIdentityDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return null;
            try
            {
                var types = DataService.GetAll<IdentityDocumentTypes>();
                return types.FirstOrDefault(t => t.Id == documentTypeId)?.Name;
            }
            catch { return null; }
        }

        private string GetPersonalDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return null;
            try
            {
                var types = DataService.GetAll<PersonalDocumentTypes>();
                return types.FirstOrDefault(t => t.Id == documentTypeId)?.Name;
            }
            catch { return null; }
        }

        /// <summary>
        /// Удаление документа из соответствующей таблицы
        /// </summary>
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
                            DataService.LogChange("EducationDocuments", item.Id, "DELETE");
                            break;
                        case "IdentityDocuments":
                            DataService.DeleteIdentityDocument(item.Id);
                            DataService.LogChange("IdentityDocuments", item.Id, "DELETE");
                            break;
                        case "Documents":
                            DataService.DeleteGeneralDocument(item.Id, SessionManager.CurrentApplicantId.Value);
                            DataService.LogChange("Documents", item.Id, "DELETE");
                            break;
                        case "AttachedDocuments":
                            DataService.DeleteAttachedDocument(item.Id);
                            DataService.LogChange("AttachedDocuments", item.Id, "DELETE");
                            break;
                    }

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
                int? applicantId = SessionManager.CurrentApplicantId ??
                    (SessionManager.CurrentApplicant?.Id != 0 ? (int?)SessionManager.CurrentApplicant?.Id : null);
                if (applicantId != null)
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
            var result = MessageBox.Show(
                "Вы уверены, что хотите отменить ввод данных?\n\n" +
                "ВНИМАНИЕ: Все данные абитуриента будут безвозвратно удалены из базы данных!\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var confirmResult = MessageBox.Show(
                    "Вы действительно хотите удалить все данные этого абитуриента?",
                    "Подтвердите удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        int? applicantId = SessionManager.CurrentApplicantId ??
                            (SessionManager.CurrentApplicant?.Id != 0 ? (int?)SessionManager.CurrentApplicant?.Id : null);
                        if (applicantId != null)
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
    }
}