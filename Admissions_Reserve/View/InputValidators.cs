using System;
using System.Windows.Controls;
using Admissions_Reserve.Model;

namespace Admissions_Reserve.View
{
    public static class InputValidators
    {
        // Helper validation methods for binding events: keep simple wrappers to ValidationHelper
        public static bool ValidateEmail(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text)) return false;
            return ValidationHelper.IsValidEmail(textBox.Text.Trim());
        }

        public static bool ValidatePhone(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text)) return false;
            return ValidationHelper.IsValidPhoneNumber(textBox.Text.Trim());
        }
    }
}
