using System;
using System.Collections.Generic;

namespace Admissions_Reserve.Model
{
    // ========== МОДЕЛИ ДЛЯ СТРАНИЦ ==========

    /// <summary>
    /// Модель для страницы с дополнительной информацией
    /// </summary>
    public class AdditionalInfoModel
    {
        public int ApplicantId { get; set; }
        public bool? NeedsDormitory { get; set; }
        public bool? HasDormitoryBenefits { get; set; }
        public bool? CompletedPreparatoryCourses { get; set; }
        public bool? CompletedPreparatoryDepartment { get; set; }
        public bool? CompletedMedicalEducation { get; set; }
        public string CurrentWorkPlace { get; set; }
        public bool? ServedInArmy { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public int? ReserveYear { get; set; }
        public string ApplicationComment { get; set; }
    }

    /// <summary>
    /// Модель языка абитуриента для страницы доп.информации
    /// </summary>
    public class LanguageViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public int LanguageLevelId { get; set; }
        public string LanguageLevelName { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Модель спортивного достижения для страницы доп.информации
    /// </summary>
    public class SportAchievementViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string SportType { get; set; }
        public string Achievement { get; set; }
        public string Rank { get; set; }
        public int? Year { get; set; }
    }

    /// <summary>
    /// Модель индивидуального достижения для страницы индивидуальных достижений
    /// </summary>
    public class IndividualAchievementViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string Achievement { get; set; }
    }

    /// <summary>
    /// Модель приоритета для страницы приоритетов
    /// </summary>
    public class ApplicationPriorityViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public int PriorityOrder { get; set; }
        public string ProgramCode { get; set; }
        public string ProgramName { get; set; }
        public string StudyForm { get; set; }
        public string EducationBase { get; set; }
        public string Department { get; set; }
        public string AdmissionType { get; set; }
        public string Branch { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Модель прикрепленного документа для страницы прикрепленных документов
    /// </summary>
    public class AttachedDocumentViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string FilePath { get; set; }
        public int FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileSizeFormatted => FormatFileSize(FileSize);

        private string FormatFileSize(int bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Модель конкурса для страницы конкурсов
    /// </summary>
    public class CompetitionPriorityViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string CompetitionName { get; set; }
        public int PriorityOrder { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Модель для контактной информации
    /// </summary>
    public class ContactInformationViewModel
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string ContactType { get; set; }
        public string ContactValue { get; set; }
        public bool IsPreferred { get; set; }
    }

    /// <summary>
    /// Модель статистики абитуриента
    /// </summary>
    public class ApplicantStatistics
    {
        public int TotalApplicants { get; set; }
        public int ApplicantsWithDocuments { get; set; }
        public int ApplicantsWithLanguages { get; set; }
        public int ApplicantsWithAchievements { get; set; }
        public int AverageDocumentsPerApplicant { get; set; }
    }
}
