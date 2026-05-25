using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Admissions_Reserve.View
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Если параметр "Inverse" передан, инвертируем результат
                if (parameter?.ToString() == "Inverse")
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                if (parameter?.ToString() == "Inverse")
                    result = !result;
                return result;
            }
            return false;
        }
    }
}