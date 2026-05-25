using System;
using System.Collections.Generic;
using System.IO;

namespace Admissions_Reserve.Model
{
    public static class LogHelper
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly object lockObject = new object();

        static LogHelper()
        {
            // Создаем директорию для логов при инициализации
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Логирование события
        /// </summary>
        public static void LogEvent(string eventType, string message, string details = "")
        {
            try
            {
                lock (lockObject)
                {
                    string logFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                    string logEntry = FormatLogEntry(eventType, message, details);

                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка логирования: {ex.Message}");
            }
        }

        /// <summary>
        /// Логирование ошибки
        /// </summary>
        public static void LogError(string message, Exception ex)
        {
            try
            {
                lock (lockObject)
                {
                    string logFile = Path.Combine(LogDirectory, $"error_{DateTime.Now:yyyy-MM-dd}.txt");
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}\n{ex.Message}\n{ex.StackTrace}\n";

                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// Логирование действия пользователя
        /// </summary>
        public static void LogUserAction(string action, string userName, string details = "")
        {
            LogEvent("USER_ACTION", $"User: {userName}, Action: {action}", details);
        }

        /// <summary>
        /// Логирование изменения данных
        /// </summary>
        public static void LogDataChange(string tableName, int recordId, string action, string changes = "")
        {
            LogEvent("DATA_CHANGE", $"Table: {tableName}, Record: {recordId}, Action: {action}", changes);
        }

        /// <summary>
        /// Логирование открытия приложения
        /// </summary>
        public static void LogApplicationStart()
        {
            LogEvent("APPLICATION", "Application started", $"Version: {GetApplicationVersion()}");
        }

        /// <summary>
        /// Логирование закрытия приложения
        /// </summary>
        public static void LogApplicationEnd()
        {
            LogEvent("APPLICATION", "Application ended", "");
        }

        /// <summary>
        /// Логирование исключения
        /// </summary>
        public static void LogException(string source, Exception ex)
        {
            try
            {
                lock (lockObject)
                {
                    string logFile = Path.Combine(LogDirectory, $"exception_{DateTime.Now:yyyy-MM-dd}.txt");
                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EXCEPTION in {source}\n";
                    logEntry += $"Message: {ex.Message}\n";
                    logEntry += $"Type: {ex.GetType().FullName}\n";
                    logEntry += $"StackTrace: {ex.StackTrace}\n";

                    if (ex.InnerException != null)
                    {
                        logEntry += $"InnerException: {ex.InnerException.Message}\n";
                    }

                    File.AppendAllText(logFile, logEntry + Environment.NewLine + new string('-', 80) + Environment.NewLine);
                }
            }
            catch { }
        }

        /// <summary>
        /// Получить все логи за день
        /// </summary>
        public static List<string> GetDayLogs(DateTime date)
        {
            var logs = new List<string>();

            try
            {
                string logFile = Path.Combine(LogDirectory, $"log_{date:yyyy-MM-dd}.txt");
                if (File.Exists(logFile))
                {
                    logs.AddRange(File.ReadAllLines(logFile));
                }
            }
            catch { }

            return logs;
        }

        /// <summary>
        /// Получить все ошибки за день
        /// </summary>
        public static List<string> GetErrorLogs(DateTime date)
        {
            var logs = new List<string>();

            try
            {
                string errorFile = Path.Combine(LogDirectory, $"error_{date:yyyy-MM-dd}.txt");
                if (File.Exists(errorFile))
                {
                    logs.AddRange(File.ReadAllLines(errorFile));
                }
            }
            catch { }

            return logs;
        }

        /// <summary>
        /// Очистить старые логи (старше указанного количества дней)
        /// </summary>
        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                var files = Directory.GetFiles(LogDirectory, "*.txt");
                var threshold = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < threshold)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Форматирование записи лога
        /// </summary>
        private static string FormatLogEntry(string eventType, string message, string details)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{eventType}] {message}{(string.IsNullOrEmpty(details) ? "" : " - " + details)}";
        }

        /// <summary>
        /// Получить версию приложения
        /// </summary>
        private static string GetApplicationVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Экспортировать логи в файл
        /// </summary>
        public static void ExportLogs(string exportPath, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var writer = new StreamWriter(exportPath))
                {
                    writer.WriteLine($"Log Export Report");
                    writer.WriteLine($"Date Range: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
                    writer.WriteLine(new string('=', 80));
                    writer.WriteLine();

                    var current = startDate.Date;
                    while (current <= endDate.Date)
                    {
                        var logs = GetDayLogs(current);
                        if (logs.Count > 0)
                        {
                            writer.WriteLine($"Logs for {current:dd.MM.yyyy}:");
                            foreach (var log in logs)
                            {
                                writer.WriteLine(log);
                            }
                            writer.WriteLine();
                        }

                        current = current.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to export logs", ex);
            }
        }
    }
}
