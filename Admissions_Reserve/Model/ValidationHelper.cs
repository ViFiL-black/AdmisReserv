using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Admissions_Reserve.Model
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Проверка email адреса
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка номера телефона
        /// </summary>
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Проверяем, содержит ли только цифры, + и -
            var regex = new Regex(@"^[\d\-\+\s\(\)]+$");
            return regex.IsMatch(phone) && phone.Length >= 10;
        }

        /// <summary>
        /// Проверка СНИЛС (11 цифр)
        /// </summary>
        public static bool IsValidSnils(string snils)
        {
            if (string.IsNullOrWhiteSpace(snils))
                return false;

            snils = snils.Trim();

            // Удаляем дефисы
            snils = snils.Replace("-", "");

            // Проверяем длину
            if (snils.Length != 11)
                return false;

            // Проверяем, что это только цифры
            return Regex.IsMatch(snils, @"^\d{11}$");
        }

        /// <summary>
        /// Проверка ИНН (10 или 12 цифр)
        /// </summary>
        public static bool IsValidInn(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn))
                return false;

            inn = inn.Trim();

            if (inn.Length != 10 && inn.Length != 12)
                return false;

            return Regex.IsMatch(inn, @"^\d{10}$|^\d{12}$");
        }

        /// <summary>
        /// Проверка паспортного номера (серия и номер)
        /// </summary>
        public static bool IsValidPassportNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            // Удаляем пробелы
            number = number.Replace(" ", "");

            // Паспорт РФ: 4 цифры серии + 6 цифр номера
            return Regex.IsMatch(number, @"^\d{10}$");
        }

        /// <summary>
        /// Проверка даты
        /// </summary>
        public static bool IsValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return false;

            return date.Value > DateTime.MinValue && date.Value <= DateTime.Now;
        }

        /// <summary>
        /// Проверка ФИО (не пусто и разумная длина)
        /// </summary>
        public static bool IsValidFullName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();
            return name.Length >= 2 && name.Length <= 100;
        }

        /// <summary>
        /// Проверка года (4 цифры)
        /// </summary>
        public static bool IsValidYear(int? year)
        {
            if (!year.HasValue)
                return false;

            var currentYear = DateTime.Now.Year;
            return year.Value >= 1900 && year.Value <= currentYear;
        }

        /// <summary>
        /// Проверка среднего балла (от 0 до 5)
        /// </summary>
        public static bool IsValidAverageScore(double score)
        {
            return score >= 0 && score <= 5;
        }

        /// <summary>
        /// Получить сообщение об ошибке валидации
        /// </summary>
        public static string GetValidationErrorMessage(string fieldName, string validationType)
        {
            var messages = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "email",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} имеет неправильный формат" }
                    }
                },
                {
                    "phone",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен содержать минимум 10 цифр" }
                    }
                },
                {
                    "snils",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен содержать 11 цифр" }
                    }
                },
                {
                    "inn",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен содержать 10 или 12 цифр" }
                    }
                },
                {
                    "passport",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен содержать 10 цифр (4 серии + 6 номера)" }
                    }
                },
                {
                    "date",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} содержит некорректную дату" }
                    }
                },
                {
                    "fullname",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} слишком короткое или слишком длинное" }
                    }
                },
                {
                    "year",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен быть от 1900 до текущего года" }
                    }
                },
                {
                    "score",
                    new Dictionary<string, string>
                    {
                        { "invalid", $"{fieldName} должен быть от 0 до 5" }
                    }
                }
            };

            if (messages.ContainsKey(validationType) && messages[validationType].ContainsKey("invalid"))
            {
                return messages[validationType]["invalid"];
            }

            return $"{fieldName} содержит ошибку";
        }
    }
}
