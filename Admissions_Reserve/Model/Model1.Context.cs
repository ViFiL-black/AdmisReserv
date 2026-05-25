//Entity Framework DbContext for SQLite
using System.Data.Entity;

namespace Admissions_Reserve.Model
{
    public partial class AdmissionsReseDBEntities : DbContext
    {
        public AdmissionsReseDBEntities()
            : base("name=AdmissionsReseDBEntities")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Applicants> Applicants { get; set; }
        public virtual DbSet<Countries> Countries { get; set; }
        public virtual DbSet<Citizenships> Citizenships { get; set; }
        public virtual DbSet<Genders> Genders { get; set; }
        public virtual DbSet<Languages> Languages { get; set; }
        public virtual DbSet<LanguageLevels> LanguageLevels { get; set; }
        public virtual DbSet<ApplicantLanguages> ApplicantLanguages { get; set; }
        public virtual DbSet<SportAchievements> SportAchievements { get; set; }
        public virtual DbSet<IndividualAchievements> IndividualAchievements { get; set; }
        public virtual DbSet<IdentityDocuments> IdentityDocuments { get; set; }
        public virtual DbSet<IdentityDocumentTypes> IdentityDocumentTypes { get; set; }
        public virtual DbSet<Applications> Applications { get; set; }
        public virtual DbSet<Relatives> Relatives { get; set; }
        public virtual DbSet<Documents> Documents { get; set; }
        public virtual DbSet<DocumentTypes> DocumentTypes { get; set; }
        public virtual DbSet<AchievementCategories> AchievementCategories { get; set; }
        public virtual DbSet<AdmissionStages> AdmissionStages { get; set; }
        public virtual DbSet<AdmissionTypes> AdmissionTypes { get; set; }
        public virtual DbSet<ApplicationCompetitions> ApplicationCompetitions { get; set; }
        public virtual DbSet<ApplicationEducationDocuments> ApplicationEducationDocuments { get; set; }
        public virtual DbSet<ApplicationPrivileges> ApplicationPrivileges { get; set; }
        public virtual DbSet<ApplicationPriorities> ApplicationPriorities { get; set; }
        public virtual DbSet<BaseEducationLevels> BaseEducationLevels { get; set; }
        public virtual DbSet<Branches> Branches { get; set; }
        public virtual DbSet<ChangeHistory> ChangeHistory { get; set; }
        public virtual DbSet<CompetitionPriorities> CompetitionPriorities { get; set; }
        public virtual DbSet<Competitions> Competitions { get; set; }
        public virtual DbSet<ContactInformation> ContactInformation { get; set; }
        public virtual DbSet<CostReimbursementTypes> CostReimbursementTypes { get; set; }
        public virtual DbSet<Departments> Departments { get; set; }
        public virtual DbSet<DocumentCategories> DocumentCategories { get; set; }
        public virtual DbSet<DocumentReceiptForms> DocumentReceiptForms { get; set; }
        public virtual DbSet<EducationalOrganizations> EducationalOrganizations { get; set; }
        public virtual DbSet<EducationDocuments> EducationDocuments { get; set; }
        public virtual DbSet<EducationDocumentTypes> EducationDocumentTypes { get; set; }
        public virtual DbSet<EducationLevels> EducationLevels { get; set; }
        public virtual DbSet<EducationPrograms> EducationPrograms { get; set; }
        public virtual DbSet<DocumentForms> DocumentForms { get; set; }
        public virtual DbSet<ApplicationTypes> ApplicationTypes { get; set; }
        public virtual DbSet<AttachedDocuments> AttachedDocuments { get; set; }
        public virtual DbSet<PersonalDocumentTypes> PersonalDocumentTypes { get; set; }
        public virtual DbSet<RelationDegrees> RelationDegrees { get; set; }
        public virtual DbSet<RelativeDocuments> RelativeDocuments { get; set; }
        public virtual DbSet<StudyForms> StudyForms { get; set; }
        public virtual DbSet<TargetAdmissionTypes> TargetAdmissionTypes { get; set; }
    }
}
