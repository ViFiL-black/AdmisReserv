using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Admissions_Reserve.Model
{
    public static class ExportHelper
    {
        /// <summary>
        /// Экспорт абитуриентов в CSV
        /// </summary>
        public static void ExportApplicantsToCSV(string filePath, List<Applicants> applicants)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Заголовок
                    var headers = new[]
                    {
                        "ID",
                        "Фамилия",
                        "Имя",
                        "Отчество",
                        "Дата рождения",
                        "Место рождения",
                        "СНИЛС",
                        "Email",
                        "Телефон",
                        "Мобильный телефон",
                        "Дата создания",
                        "Дата изменения"
                    };

                    writer.WriteLine(string.Join(";", headers));

                    // Данные
                    foreach (var applicant in applicants)
                    {
                        var values = new[]
                        {
                            applicant.Id.ToString(),
                            applicant.LastName ?? "",
                            applicant.FirstName ?? "",
                            applicant.Patronymic ?? "",
                            applicant.BirthDate.HasValue ? applicant.BirthDate.Value.ToString("dd.MM.yyyy") : "",
                            applicant.BirthPlace ?? "",
                            applicant.Snils ?? "",
                            applicant.Email ?? "",
                            applicant.Phone ?? "",
                            applicant.MobilePhone ?? "",
                            applicant.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "",
                            applicant.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? ""
                        };

                        writer.WriteLine(string.Join(";", values));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте в CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Экспорт документов абитуриента в CSV
        /// </summary>
        public static void ExportEducationDocumentsToCSV(string filePath, List<EducationDocuments> documents)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    var headers = new[]
                    {
                        "ID",
                        "ID абитуриента",
                        "Образовательное учреждение",
                        "Город",
                        "Серия",
                        "Номер",
                        "Дата выдачи",
                        "Год окончания",
                        "Средний балл",
                        "Дата создания"
                    };

                    writer.WriteLine(string.Join(";", headers));

                    foreach (var doc in documents)
                    {
                        var values = new[]
                        {
                            doc.Id.ToString(),
                            doc.ApplicantId.ToString(),
                            doc.EducationalOrg ?? "",
                            doc.City ?? "",
                            doc.Series ?? "",
                            doc.Number ?? "",
                            doc.IssueDate?.ToString("dd.MM.yyyy") ?? "",
                            doc.GraduationYear?.ToString("yyyy") ?? "",
                            doc.AverageScore.ToString("F2"),
                            doc.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                        };

                        writer.WriteLine(string.Join(";", values));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте документов: {ex.Message}");
            }
        }

        /// <summary>
        /// Экспорт приоритетов в CSV
        /// </summary>
        public static void ExportPrioritiesToCSV(string filePath, List<ApplicationPriorities> priorities)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    var headers = new[]
                    {
                        "ID",
                        "ID абитуриента",
                        "Приоритет",
                        "Код программы",
                        "Название программы",
                        "Форма обучения",
                        "Основа обучения",
                        "Кафедра",
                        "Тип поступления"
                    };

                    writer.WriteLine(string.Join(";", headers));

                    foreach (var priority in priorities)
                    {
                        var values = new[]
                        {
                            priority.Id.ToString(),
                            priority.ApplicantId.ToString(),
                            priority.PriorityOrder.ToString(),
                            priority.ProgramCode ?? "",
                            priority.ProgramName ?? "",
                            priority.StudyForm ?? "",
                            priority.EducationBase ?? "",
                            priority.Department ?? "",
                            priority.AdmissionType ?? ""
                        };

                        writer.WriteLine(string.Join(";", values));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте приоритетов: {ex.Message}");
            }
        }

        /// <summary>
        /// Экспорт в HTML отчет
        /// </summary>
        public static void ExportToHTML(string filePath, string title, string content)
        {
            try
            {
                var html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine($"<title>{title}</title>");
                html.AppendLine("<meta charset=\"UTF-8\">");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                html.AppendLine("h1 { color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }");
                html.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
                html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
                html.AppendLine("th { background-color: #007bff; color: white; }");
                html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
                html.AppendLine("tr:hover { background-color: #f5f5f5; }");
                html.AppendLine(".footer { margin-top: 40px; border-top: 1px solid #ddd; padding-top: 10px; font-size: 12px; color: #666; }");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                html.AppendLine($"<h1>{title}</h1>");
                html.AppendLine(content);
                html.AppendLine("<div class=\"footer\">");
                html.AppendLine($"<p>Отчет создан: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");
                html.AppendLine("</div>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");

                File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте в HTML: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить HTML таблицу абитуриентов
        /// </summary>
        public static string GetApplicantsHTML(List<Applicants> applicants)
        {
            var html = new StringBuilder();
            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>Фамилия</th>");
            html.AppendLine("<th>Имя</th>");
            html.AppendLine("<th>Отчество</th>");
            html.AppendLine("<th>Email</th>");
            html.AppendLine("<th>Телефон</th>");
            html.AppendLine("<th>Дата создания</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var applicant in applicants)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{applicant.LastName}</td>");
                html.AppendLine($"<td>{applicant.FirstName}</td>");
                html.AppendLine($"<td>{applicant.Patronymic}</td>");
                html.AppendLine($"<td>{applicant.Email}</td>");
                html.AppendLine($"<td>{applicant.Phone}</td>");
                html.AppendLine($"<td>{applicant.CreatedAt?.ToString("dd.MM.yyyy")}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            return html.ToString();
        }

        /// <summary>
        /// Получить статистику в HTML
        /// </summary>
        public static string GetStatisticsHTML()
        {
            var stats = new StringBuilder();

            try
            {
                var totalApplicants = DataService.GetTotalApplicantsCount();
                var withDocuments = DataService.GetApplicantsWithDocumentsCount();
                var withLanguages = DataService.GetApplicantsWithLanguagesCount();
                var withAchievements = DataService.GetApplicantsWithAchievementsCount();

                stats.AppendLine("<h2>Статистика</h2>");
                stats.AppendLine("<table>");
                stats.AppendLine("<tr><td><strong>Всего абитуриентов</strong></td><td>" + totalApplicants + "</td></tr>");
                stats.AppendLine("<tr><td><strong>С документами об образовании</strong></td><td>" + withDocuments + "</td></tr>");
                stats.AppendLine("<tr><td><strong>С языками</strong></td><td>" + withLanguages + "</td></tr>");
                stats.AppendLine("<tr><td><strong>С достижениями</strong></td><td>" + withAchievements + "</td></tr>");
                stats.AppendLine("</table>");
            }
            catch (Exception ex)
            {
                stats.AppendLine($"<p>Ошибка при получении статистики: {ex.Message}</p>");
            }

            return stats.ToString();
        }
    }
}
