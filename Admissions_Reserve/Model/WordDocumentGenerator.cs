using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace Admissions_Reserve.Model
{
    public static class WordDocumentGenerator
    {
        private static readonly string TemplatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");

        /// <summary>
        /// Заполняет шаблон Word, заменяя закладки значениями из словаря.
        /// </summary>
        private static void FillTemplate(string templateFileName, string savePath,
            Dictionary<string, string> bookmarkValues, bool showWord = false)
        {
            string templatePath = Path.Combine(TemplatesFolder, templateFileName);
            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Шаблон не найден: {templatePath}");

            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                wordApp = new Word.Application();
                wordApp.Visible = showWord;
                wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                doc = wordApp.Documents.Open(templatePath);

                foreach (var kv in bookmarkValues)
                {
                    if (doc.Bookmarks.Exists(kv.Key))
                    {
                        Word.Range range = doc.Bookmarks[kv.Key].Range;
                        range.Text = kv.Value ?? "";
                        doc.Bookmarks.Add(kv.Key, range); // восстанавливаем закладку
                    }
                }

                doc.SaveAs2(savePath);
            }
            finally
            {
                if (doc != null) doc.Close(false);
                if (wordApp != null) wordApp.Quit();
                if (doc != null) Marshal.ReleaseComObject(doc);
                if (wordApp != null) Marshal.ReleaseComObject(wordApp);
            }
        }

        // ------------------- Заявление -------------------
        public static void GenerateApplication(int applicantId, string savePath)
        {
            var data = GetExtendedApplicationData(applicantId);
            FillTemplate("ApplicationTemplate.docx", savePath, data);
        }

        private static Dictionary<string, string> GetExtendedApplicationData(int applicantId)
        {
            var app = DataService.GetApplicant(applicantId);
            if (app == null) throw new Exception("Абитуриент не найден");

            var identity = DataService.GetApplicantDocuments(applicantId).FirstOrDefault();
            var eduDoc = DataService.GetApplicantEducationDocuments(applicantId).FirstOrDefault();
            var priorities = DataService.GetApplicantPriorities(applicantId);
            // Берём первый приоритет (с наименьшим номером PriorityOrder)
            var firstPriority = priorities.OrderBy(p => p.PriorityOrder).FirstOrDefault();
            var competition = firstPriority != null
                ? DataService.GetByCondition<Competitions>("Name = @Name",
                    new SQLiteParameter("@Name", firstPriority.ProgramName)).FirstOrDefault()
                : null;

            var relatives = DataService.GetApplicantRelatives(applicantId);
            var mother = relatives.FirstOrDefault(r => r.RelationDegree == "Мать");
            var father = relatives.FirstOrDefault(r => r.RelationDegree == "Отец");

            var languages = DataService.GetApplicantLanguages(applicantId);
            var languageNames = languages.Select(l => GetLanguageName(l.LanguageId)).Where(n => !string.IsNullOrEmpty(n));
            string foreignLanguages = string.Join(", ", languageNames);

            var achievements = DataService.GetApplicantIndividualAchievements(applicantId);
            string achievementsList = string.Join("; ", achievements.Select(a => a.AchievementName ?? a.Achievement ?? ""));

            string needsDorm = app.NeedsDormitory == true ? "нуждаюсь" : "не нуждаюсь";
            string workExp = !string.IsNullOrEmpty(app.CurrentWorkPlace) ? app.CurrentWorkPlace : "";
            string photoCount = "___";

            return new Dictionary<string, string>
    {
        { "FullName", $"{app.LastName} {app.FirstName} {app.Patronymic}".Trim() },
        { "Citizenship", GetCitizenshipName(app.CitizenshipId) },
        { "PassportSeries", identity?.Series ?? "" },
        { "PassportNumber", identity?.Number ?? "" },
        { "PassportIssuedBy", identity?.IssuedBy ?? "" },
        { "PassportIssueDate", identity?.IssueDate?.ToString("dd.MM.yyyy") ?? "" },
        { "BirthDate", app.BirthDate?.ToString("dd.MM.yyyy") ?? "" },
        { "Age", app.BirthDate.HasValue ? (DateTime.Now.Year - app.BirthDate.Value.Year).ToString() : "" },
        { "Gender", app.GenderId == 1 ? "Мужской" : "Женский" },
        { "BirthPlace", app.BirthPlace ?? "" },
        { "Snils", app.Snils ?? "" },
        { "RegistrationAddress", FormatAddress(
            app.RegistrationPostalCode, app.RegistrationRegion, app.RegistrationDistrict,
            app.RegistrationCity, app.RegistrationStreet, app.RegistrationHouse,
            app.RegistrationBuilding, app.RegistrationApartment) },
        { "ActualAddress", FormatAddress(
            app.ActualPostalCode, app.ActualRegion, app.ActualDistrict,
            app.ActualCity, app.ActualStreet, app.ActualHouse,
            app.ActualBuilding, app.ActualApartment) },
        { "Phone", app.MobilePhone ?? app.Phone ?? "" },
        { "Email", app.Email ?? "" },
        // Специальность, форма обучения и база – из первого приоритета
        { "Specialty", competition?.Name ?? firstPriority?.ProgramName ?? "" },
        { "StudyForm", competition?.StudyForm ?? firstPriority?.StudyForm ?? "" },
        { "EducationBase", competition?.EducationBase ?? firstPriority?.EducationBase ?? "" },
        { "Education", eduDoc?.EducationalOrg ?? "" },
        { "GraduationYear", eduDoc?.GraduationYear?.Year.ToString() ?? "" },
        { "DocumentSeries", eduDoc?.Series ?? "" },
        { "DocumentNumber", eduDoc?.Number ?? "" },
        { "AverageScore", eduDoc?.AverageScore.ToString("F2") ?? "" },
        { "MotherInfo", mother != null ? $"{mother.LastName} {mother.FirstName} {mother.Patronymic}, {mother.Phone}" : "" },
        { "FatherInfo", father != null ? $"{father.LastName} {father.FirstName} {father.Patronymic}, {father.Phone}" : "" },
        { "WorkExperience", workExp },
        { "PhotoCount", photoCount },
        { "NeedsDormitory", needsDorm },
        { "ForeignLanguages", foreignLanguages },
        { "IndividualAchievements", achievementsList },
        { "ApplicationDate", DateTime.Now.ToString("dd.MM.yyyy") }
    };
        }

        // ------------------- Согласие абитуриента -------------------
        public static void GenerateConsent(int applicantId, string savePath)
        {
            var data = GetConsentBookmarkData(applicantId);
            FillTemplate("ConsentTemplate.docx", savePath, data);
        }

        private static Dictionary<string, string> GetConsentBookmarkData(int applicantId)
        {
            var app = DataService.GetApplicant(applicantId);
            if (app == null) throw new Exception("Абитуриент не найден");
            var identity = DataService.GetApplicantDocuments(applicantId).FirstOrDefault();

            return new Dictionary<string, string>
            {
                { "FullName", $"{app.LastName} {app.FirstName} {app.Patronymic}".Trim() },
                { "RegistrationAddress", FormatAddress(
                    app.RegistrationPostalCode, app.RegistrationRegion, app.RegistrationDistrict,
                    app.RegistrationCity, app.RegistrationStreet, app.RegistrationHouse,
                    app.RegistrationBuilding, app.RegistrationApartment) },
                { "PassportSeries", identity?.Series ?? "" },
                { "PassportNumber", identity?.Number ?? "" },
                { "PassportIssuedBy", identity?.IssuedBy ?? "" },
                { "PassportIssueDate", identity?.IssueDate?.ToString("dd.MM.yyyy") ?? "" },
                { "BirthDate", app.BirthDate?.ToString("dd.MM.yyyy") ?? "" },
                { "BirthPlace", app.BirthPlace ?? "" },
                { "Citizenship", GetCitizenshipName(app.CitizenshipId) },
                { "Snils", app.Snils ?? "" },
                { "Phone", app.MobilePhone ?? app.Phone ?? "" },
                { "Email", app.Email ?? "" }
            };
        }

        // ------------------- Согласие законного представителя -------------------
        public static void GenerateGuardianConsent(int applicantId, string savePath)
        {
            var data = GetGuardianBookmarkData(applicantId);
            if (data == null)
                throw new Exception("Абитуриент совершеннолетний или не найден законный представитель.");
            FillTemplate("GuardianConsentTemplate.docx", savePath, data);
        }

        private static Dictionary<string, string> GetGuardianBookmarkData(int applicantId)
        {
            var app = DataService.GetApplicant(applicantId);
            if (app == null) throw new Exception("Абитуриент не найден");
            if (app.BirthDate.HasValue && DateTime.Now.Year - app.BirthDate.Value.Year >= 18)
                return null;

            var relatives = DataService.GetApplicantRelatives(applicantId);
            var guardian = relatives.FirstOrDefault(r =>
                r.RelationDegree == "Отец" || r.RelationDegree == "Мать" || r.RelationDegree == "Опекун");
            if (guardian == null) throw new Exception("Законный представитель не найден");

            return new Dictionary<string, string>
            {
                { "GuardianFullName", $"{guardian.LastName} {guardian.FirstName} {guardian.Patronymic}".Trim() },
                { "GuardianBirthDate", guardian.BirthDate?.ToString("dd.MM.yyyy") ?? "" },
                { "GuardianPassportSeries", guardian.IdNumber?.Length >= 4 ? guardian.IdNumber.Substring(0,4) : "" },
                { "GuardianPassportNumber", guardian.IdNumber?.Length > 4 ? guardian.IdNumber.Substring(4) : (guardian.IdNumber ?? "") },
                { "GuardianPassportIssuedBy", guardian.IssuedBy ?? "" },
                { "GuardianPassportIssueDate", guardian.IssueDate?.ToString("dd.MM.yyyy") ?? "" },
                { "GuardianRegistrationAddress", guardian.FiasAddress ?? "" },
                { "GuardianPhone", guardian.Phone ?? "" },
                { "GuardianEmail", guardian.Email ?? "" },
                { "ChildFullName", $"{app.LastName} {app.FirstName} {app.Patronymic}".Trim() },
                { "ChildBirthDate", app.BirthDate?.ToString("dd.MM.yyyy") ?? "" },
                { "ChildBirthPlace", app.BirthPlace ?? "" },
                { "ChildCitizenship", GetCitizenshipName(app.CitizenshipId) },
                { "ChildRegistrationAddress", FormatAddress(
                    app.RegistrationPostalCode, app.RegistrationRegion, app.RegistrationDistrict,
                    app.RegistrationCity, app.RegistrationStreet, app.RegistrationHouse,
                    app.RegistrationBuilding, app.RegistrationApartment) },
                { "ChildSnils", app.Snils ?? "" }
            };
        }

        // ------------------- Вспомогательные методы -------------------
        private static string GetCitizenshipName(int? citizenshipId)
        {
            if (citizenshipId == null) return "";
            var citizenships = DataService.GetAll<Citizenships>();
            return citizenships.FirstOrDefault(c => c.Id == citizenshipId)?.Name ?? "";
        }

        private static string GetLanguageName(int? languageId)
        {
            if (languageId == null) return "";
            var languages = DataService.GetAll<Languages>();
            return languages.FirstOrDefault(l => l.Id == languageId)?.Name ?? "";
        }

        private static string FormatAddress(string postal, string region, string district, string city,
            string street, string house, string building, string apartment)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(postal)) parts.Add(postal);
            if (!string.IsNullOrEmpty(region)) parts.Add(region);
            if (!string.IsNullOrEmpty(district)) parts.Add(district);
            if (!string.IsNullOrEmpty(city)) parts.Add(city);
            if (!string.IsNullOrEmpty(street)) parts.Add(street);
            if (!string.IsNullOrEmpty(house)) parts.Add($"д. {house}");
            if (!string.IsNullOrEmpty(building)) parts.Add($"корп. {building}");
            if (!string.IsNullOrEmpty(apartment)) parts.Add($"кв. {apartment}");
            return string.Join(", ", parts);
        }
    }
}