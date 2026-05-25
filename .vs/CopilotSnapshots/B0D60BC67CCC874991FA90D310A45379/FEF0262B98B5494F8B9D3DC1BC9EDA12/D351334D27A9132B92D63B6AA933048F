// Models.cs
using System;
using System.Collections.Generic;

namespace Admissions_Reserve.Model
{
    // ========== АБИТУРИЕНТЫ ==========
    public partial class Applicants
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public int? GenderId { get; set; }
        public int? CitizenshipId { get; set; }
        public string Snils { get; set; }
        public string Inn { get; set; }
        public string Email { get; set; }
        public string AdditionalEmail { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string Telegram { get; set; }
        public string WhatsApp { get; set; }
        public string Viber { get; set; }
        public string PreferredContactMethod { get; set; }
        public string ContactComment { get; set; }

        // Адрес регистрации
        public int? RegistrationCountryId { get; set; }
        public string RegistrationPostalCode { get; set; }
        public string RegistrationRegion { get; set; }
        public string RegistrationDistrict { get; set; }
        public string RegistrationCity { get; set; }
        public string RegistrationStreet { get; set; }
        public string RegistrationHouse { get; set; }
        public string RegistrationBuilding { get; set; }
        public string RegistrationApartment { get; set; }

        // Фактический адрес
        public bool? ActualAddressSame { get; set; }
        public int? ActualCountryId { get; set; }
        public string ActualPostalCode { get; set; }
        public string ActualRegion { get; set; }
        public string ActualDistrict { get; set; }
        public string ActualCity { get; set; }
        public string ActualStreet { get; set; }
        public string ActualHouse { get; set; }
        public string ActualBuilding { get; set; }
        public string ActualApartment { get; set; }

        // Дополнительные поля
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

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ApplicantLanguages> ApplicantLanguages { get; set; }
        public virtual ICollection<Applications> Applications { get; set; }
        public virtual ICollection<Documents> Documents { get; set; }
        public virtual ICollection<IdentityDocuments> IdentityDocuments { get; set; }
        public virtual ICollection<IndividualAchievements> IndividualAchievements { get; set; }
        public virtual ICollection<Relatives> Relatives { get; set; }
        public virtual ICollection<SportAchievements> SportAchievements { get; set; }

        public virtual Countries Countries { get; set; }
        public virtual Citizenships Citizenships { get; set; }
        public virtual Genders Genders { get; set; }
    }

    // ========== УДОСТОВЕРЕНИЯ ЛИЧНОСТИ ==========
    public partial class IdentityDocuments
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string IssuedBy { get; set; }
        public DateTime? IssueDate { get; set; }
        public string DepartmentCode { get; set; }
        public bool? IsPrimary { get; set; }
        public DateTime? AddedDate { get; set; }

        public virtual Applicants Applicants { get; set; }
        public virtual IdentityDocumentTypes IdentityDocumentTypes { get; set; }
    }

    // ========== ТИПЫ ДОКУМЕНТОВ ==========
    public partial class IdentityDocumentTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ========== ГРАЖДАНСТВА ==========
    public partial class Citizenships
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ========== СТРАНЫ ==========
    public partial class Countries
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    // ========== ПОЛ ==========
    public partial class Genders
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ========== ИСТОРИЯ ИЗМЕНЕНИЙ ==========
    public partial class ChangeHistory
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    // ========== ЯЗЫКИ ==========
    public partial class Languages
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ========== УРОВНИ ВЛАДЕНИЯ ЯЗЫКОМ ==========
    public partial class LanguageLevels
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
    }

    // ========== ЯЗЫКИ АБИТУРИЕНТА ==========
    public partial class ApplicantLanguages
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public int? LanguageId { get; set; }
        public int? LanguageLevelId { get; set; }
        public bool? IsPrimary { get; set; }
    }

    // ========== ДОКУМЕНТЫ ==========
    public partial class Documents
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
    }

    // ========== ТИПЫ ДОКУМЕНТОВ ==========
    public partial class DocumentTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ========== ПРИЛОЖЕНИЯ ==========
    public partial class Applications
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public string ApplicationNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // ========== ДОСТИЖЕНИЯ ==========
    public partial class SportAchievements
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public string Achievement { get; set; }
        public string SportType { get; set; }
        public string Rank { get; set; }
        public int? Year { get; set; }
    }

    public partial class IndividualAchievements
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public string Achievement { get; set; }
    }

    // ========== РОДСТВЕННИКИ ==========
    public partial class Relatives
    {
        public int Id { get; set; }
        public int? ApplicantId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ==========
    public partial class AchievementCategories
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class AdmissionStages
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class AdmissionTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class ApplicationCompetitions
    {
        public int Id { get; set; }
        public int? ApplicationId { get; set; }
    }

    public partial class ApplicationEducationDocuments
    {
        public int Id { get; set; }
        public int? ApplicationId { get; set; }
    }

    public partial class ApplicationPrivileges
    {
        public int Id { get; set; }
        public int? ApplicationId { get; set; }
    }

    public partial class BaseEducationLevels
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class Branches
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class Competitions
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class CostReimbursementTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class Departments
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class DocumentCategories
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class DocumentReceiptForms
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class EducationalOrganizations
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class EducationLevels
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class EducationPrograms
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class PersonalDocumentTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class RelationDegrees
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class RelativeDocuments
    {
        public int Id { get; set; }
        public int? RelativeId { get; set; }
    }

    public partial class StudyForms
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class TargetAdmissionTypes
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}