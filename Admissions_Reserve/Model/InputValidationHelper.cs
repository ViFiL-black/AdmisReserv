using System;

namespace Admissions_Reserve.Model
{
    // Lightweight compatibility wrapper for older projects expecting InputValidationHelper
    public static class InputValidationHelper
    {
        public static bool IsValidEmail(string email) => ValidationHelper.IsValidEmail(email);
        public static bool IsValidPhoneNumber(string phone) => ValidationHelper.IsValidPhoneNumber(phone);
        public static bool IsValidSnils(string snils) => ValidationHelper.IsValidSnils(snils);
        public static bool IsValidInn(string inn) => ValidationHelper.IsValidInn(inn);
        public static bool IsValidPassportNumber(string number) => ValidationHelper.IsValidPassportNumber(number);
        public static bool IsValidDate(DateTime? date) => ValidationHelper.IsValidDate(date);
    }
}
