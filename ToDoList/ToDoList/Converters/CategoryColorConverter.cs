using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ToDoList.Models;

namespace ToDoList.Converters
{
    public class CategoryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorHex)
            {
                try
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex);
                }
                catch
                {
                    return Brushes.Gray;
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}