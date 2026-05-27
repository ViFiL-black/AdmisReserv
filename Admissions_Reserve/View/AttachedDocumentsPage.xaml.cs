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
        public class AttachedDocument : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string DocumentType { get; set; }
            public string SeriesNumber { get; set; }
            public string Category { get; set; }
            public DateTime? IssueDate { get; set; }
            public string DocumentInfo { get; set; }

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
            LoadMainDocuments();
            LoadAttachedDocumentsFromDB();
        }

        private void LoadMainDocuments()
        {
            try
            {
                if (SessionManager.CurrentApplicantId == null) return;

                // Загружаем документ об образовании
                var eduDocs = DataService.GetApplicantEducationDocuments(SessionManager.CurrentApplicantId.Value);
                var eduDoc = eduDocs.FirstOrDefault();
                if (eduDoc != null)
                {
                    EduDocType.Text = GetDocumentTypeName(eduDoc.DocumentTypeId) ?? "Документ об образовании";
                    EduDocDate.Text = eduDoc.IssueDate?.ToString("dd.MM.yyyy") ?? "-";
                    EduDocSeriesNumber.Text = $"{eduDoc.Series} {eduDoc.Number}".Trim();
                    EduDocOrg.Text = eduDoc.EducationalOrg ?? "-";
                }

                // Загружаем удостоверение личности
                var idDocs = DataService.GetAllIdentityDocuments(SessionManager.CurrentApplicantId.Value);
                var idDoc = idDocs.FirstOrDefault(d => d.IsPrimary == true) ?? idDocs.FirstOrDefault();
                if (idDoc != null)
                {
                    var docTypes = DataService.GetAll<IdentityDocumentTypes>();
                    var docType = docTypes.FirstOrDefault(dt => dt.Id == idDoc.DocumentTypeId);
                    IdDocType.Text = docType?.Name ?? "Паспорт";
                    IdDocDate.Text = idDoc.IssueDate?.ToString("dd.MM.yyyy") ?? "-";
                    IdDocSeriesNumber.Text = $"{idDoc.Series} {idDoc.Number}".Trim();
                    IdDocIssuedBy.Text = idDoc.IssuedBy ?? "-";
                }
            }
            catch { }
        }

        private string GetDocumentTypeName(int? documentTypeId)
        {
            if (documentTypeId == null) return null;
            try
            {
                var types = DataService.GetAll<EducationDocumentTypes>();
                return types.FirstOrDefault(t => t.Id == documentTypeId)?.Name;
            }
            catch { return null; }
        }

        private void LoadAttachedDocumentsFromDB()
        {
            try
            {
                if (SessionManager.CurrentApplicantId == null) return;
                _documents.Clear();
                _nextNumber = 1;

                var docs = DataService.GetApplicantAttachedDocuments(SessionManager.CurrentApplicantId.Value);
                foreach (var doc in docs)
                {
                    _documents.Add(new AttachedDocument
                    {
                        Id = doc.Id,
                        Number = _nextNumber++,
                        DocumentType = doc.DocumentName ?? doc.DocumentType ?? "",
                        SeriesNumber = "",
                        Category = "",
                        IssueDate = doc.UploadedAt,
                        DocumentInfo = ""
                    });
                }
                DocumentsGrid.Items.Refresh();
            }
            catch { }
        }

        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is AttachedDocument item)
            {
                if (MessageBox.Show($"Удалить \"{item.DocumentType}\"?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (item.Id > 0)
                        DataService.DeleteAttachedDocument(item.Id);
                    _documents.Remove(item);
                    RenumberDocuments();
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
                // Сохраняем все данные перед завершением
                if (SessionManager.CurrentApplicantId != null)
                {
                    var applicant = DataService.GetApplicant(SessionManager.CurrentApplicantId.Value);
                    if (applicant != null)
                    {
                        applicant.UpdatedAt = DateTime.Now;
                        DataService.UpdateApplicant(applicant);
                    }
                    DataService.LogChange("Applicants", SessionManager.CurrentApplicantId.Value, "COMPLETE");
                }

                MessageBox.Show("Заявление успешно заполнено! Все данные сохранены.\nВы будете перенаправлены на страницу поиска абитуриентов.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                SessionManager.Clear();

                // Переход на страницу поиска абитуриентов (без выхода из аккаунта)
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
                // Двойное подтверждение
                var confirmResult = MessageBox.Show(
                    "Вы действительно хотите удалить все данные этого абитуриента?",
                    "Подтвердите удаление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем все данные абитуриента из БД
                        if (SessionManager.CurrentApplicantId != null)
                        {
                            DataService.DeleteApplicant(SessionManager.CurrentApplicantId.Value);
                            DataService.LogChange("Applicants", SessionManager.CurrentApplicantId.Value, "DELETE_ALL");
                        }

                        SessionManager.Clear();

                        MessageBox.Show("Все данные абитуриента удалены.\nВы будете перенаправлены на страницу поиска.",
                            "Удалено", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Переход на страницу поиска абитуриентов
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