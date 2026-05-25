using System;

namespace Admissions_Reserve.Model
{
    public static class FormattingHelper
    {
        /// <summary>
        /// Форматирование полного имени
        /// </summary>
        public static string FormatFullName(string firstName, string lastName, string patronymic)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(lastName))
                parts.Add(lastName);
            if (!string.IsNullOrWhiteSpace(firstName))
                parts.Add(firstName);
            if (!string.IsNullOrWhiteSpace(patronymic))
                parts.Add(patronymic);

            return string.Join(" ", parts);
        }

        /// <summary>
        /// Форматирование полного адреса
        /// </summary>
        public static string FormatAddress(string country, string region, string district, 
                                           string city, string street, string house, 
                                           string building, string apartment)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(country))
                parts.Add($"Страна: {country}");
            if (!string.IsNullOrWhiteSpace(region))
                parts.Add($"Область: {region}");
            if (!string.IsNullOrWhiteSpace(district))
                parts.Add($"Район: {district}");
            if (!string.IsNullOrWhiteSpace(city))
                parts.Add($"Город: {city}");
            if (!string.IsNullOrWhiteSpace(street))
                parts.Add($"Улица: {street}");
            if (!string.IsNullOrWhiteSpace(house))
                parts.Add($"Дом: {house}");
            if (!string.IsNullOrWhiteSpace(building))
                parts.Add($"Корпус: {building}");
            if (!string.IsNullOrWhiteSpace(apartment))
                parts.Add($"Кв./оф: {apartment}");

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Форматирование телефонного номера
        /// </summary>
        public static string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            // Удаляем все не-цифры
            var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");

            if (digits.Length == 0)
                return phone;

            // Если количество цифр >= 10, форматируем как +7 (XXX) XXX-XX-XX
            if (digits.Length >= 10)
            {
                if (digits.StartsWith("7"))
                    digits = digits.Substring(1);

                if (digits.Length == 10)
                {
                    return $"+7 ({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 2)}-{digits.Substring(8, 2)}";
                }
            }

            return phone;
        }

        /// <summary>
        /// Форматирование СНИЛС
        /// </summary>
        public static string FormatSnils(string snils)
        {
            if (string.IsNullOrWhiteSpace(snils))
                return "";

            var digits = System.Text.RegularExpressions.Regex.Replace(snils, @"\D", "");

            if (digits.Length == 11)
            {
                return $"{digits.Substring(0, 3)}-{digits.Substring(3, 3)}-{digits.Substring(6, 3)} {digits.Substring(9, 2)}";
            }

            return snils;
        }

        /// <summary>
        /// Форматирование ИНН
        /// </summary>
        public static string FormatInn(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn))
                return "";

            var digits = System.Text.RegularExpressions.Regex.Replace(inn, @"\D", "");

            if (digits.Length == 10)
            {
                return $"{digits.Substring(0, 2)}{digits.Substring(2, 2)} {digits.Substring(4, 3)}{digits.Substring(7, 3)}";
            }
            else if (digits.Length == 12)
            {
                return $"{digits.Substring(0, 2)} {digits.Substring(2, 2)} {digits.Substring(4, 3)} {digits.Substring(7, 2)} {digits.Substring(9, 3)}";
            }

            return inn;
        }

        /// <summary>
        /// Форматирование номера паспорта
        /// </summary>
        public static string FormatPassportNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return "";

            var digits = System.Text.RegularExpressions.Regex.Replace(number, @"\D", "");

            if (digits.Length == 10)
            {
                return $"{digits.Substring(0, 4)} {digits.Substring(4, 6)}";
            }

            return number;
        }

        /// <summary>
        /// Форматирование среднего балла
        /// </summary>
        public static string FormatAverageScore(double score)
        {
            return score.ToString("F2");
        }

        /// <summary>
        /// Форматирование размера файла
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Форматирование размера файла (перегрузка для int)
        /// </summary>
        public static string FormatFileSize(int bytes)
        {
            return FormatFileSize((long)bytes);
        }

        /// <summary>
        /// Форматирование даты и времени
        /// </summary>
        public static string FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "";

            return dateTime.Value.ToString("dd.MM.yyyy HH:mm:ss");
        }

        /// <summary>
        /// Форматирование даты
        /// </summary>
        public static string FormatDate(DateTime? date)
        {
            if (!date.HasValue)
                return "";

            return date.Value.ToString("dd.MM.yyyy");
        }

        /// <summary>
        /// Форматирование года
        /// </summary>
        public static string FormatYear(DateTime? date)
        {
            if (!date.HasValue)
                return "";

            return date.Value.ToString("yyyy");
        }

        /// <summary>
        /// Форматирование логического значения
        /// </summary>
        public static string FormatBoolean(bool? value)
        {
            if (!value.HasValue)
                return "";

            return value.Value ? "Да" : "Нет";
        }

        /// <summary>
        /// Форматирование периода дат
        /// </summary>
        public static string FormatDateRange(DateTime? startDate, DateTime? endDate)
        {
            var start = FormatDate(startDate);
            var end = FormatDate(endDate);

            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                return $"{start} - {end}";
            }

            return !string.IsNullOrEmpty(start) ? start : (!string.IsNullOrEmpty(end) ? end : "");
        }

        /// <summary>
        /// Форматирование контактов
        /// </summary>
        public static string FormatContact(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
                return "";

            return $"{type}: {value}";
        }

        /// <summary>
        /// Форматирование СНИЛС для отображения (маскирование)
        /// </summary>
        public static string MaskSnils(string snils)
        {
            if (string.IsNullOrWhiteSpace(snils) || snils.Length < 6)
                return snils;

            var digits = System.Text.RegularExpressions.Regex.Replace(snils, @"\D", "");

            if (digits.Length >= 11)
            {
                return $"{digits.Substring(0, 3)}-***-*** **";
            }

            return snils;
        }

        /// <summary>
        /// Форматирование номера паспорта для отображения (маскирование)
        /// </summary>
        public static string MaskPassport(string number)
        {
            if (string.IsNullOrWhiteSpace(number) || number.Length < 8)
                return number;

            var digits = System.Text.RegularExpressions.Regex.Replace(number, @"\D", "");

            if (digits.Length >= 10)
            {
                return $"{digits.Substring(0, 4)} ****";
            }

            return number;
        }

        /// <summary>
        /// Форматирование Email (маскирование)
        /// </summary>
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length > 3)
            {
                var masked = username.Substring(0, 2) + new string('*', username.Length - 2);
                return $"{masked}@{domain}";
            }

            return email;
        }

        /// <summary>
        /// Преобразование текста в заглавные буквы (для ФИО)
        /// </summary>
        public static string ToPascalCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        /// <summary>
        /// Получить инициалы
        /// </summary>
        public static string GetInitials(string firstName, string patronymic)
        {
            var initials = "";

            if (!string.IsNullOrWhiteSpace(firstName))
                initials += firstName[0].ToString().ToUpper();

            if (!string.IsNullOrWhiteSpace(patronymic))
                initials += patronymic[0].ToString().ToUpper();

            return initials;
        }
    }
}
