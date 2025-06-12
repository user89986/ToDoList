using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ToDoList.Converters
{
    public class BooleanToThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool isDark && isDark) ? BaseTheme.Dark : BaseTheme.Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BaseTheme theme)
            {
                return theme == BaseTheme.Dark;
            }
            return false;
        }
    }
}